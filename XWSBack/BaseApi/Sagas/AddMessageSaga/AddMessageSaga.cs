using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Chats.Messages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.AddMessageSaga
{
    public class AddMessageSaga : Saga<AddMessageSagaData>,
        IAmStartedByMessages<BeginAddMessageRequest>,
        IHandleMessages<AddMessageResponse>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public AddMessageSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AddMessageSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginAddMessageRequest>(m => m.CorrelationId)
                .ToMessage<AddMessageResponse>(m => m.CorrelationId)
                .ToMessage<GetFollowStatsResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginAddMessageRequest message, IMessageHandlerContext context)
        {
            Data.Message = message.Message;
            Data.CorrelationId = message.CorrelationId;
            Data.DateCreated = message.DateCreated;
            Data.SenderId = message.SenderId;
            Data.ReceiverId = message.ReceiverId;

            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.SenderId
            }).ConfigureAwait(false);
        }

        public async Task Handle(AddMessageResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new NewMessageNotification()
            {
                Message = Data.Message,
                DateCreated = Data.DateCreated,
                ReceiverId = Data.ReceiverId,
                SenderId = Data.SenderId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            if (!message.Following.Contains(Data.ReceiverId) || !message.Followers.Contains(Data.ReceiverId))
            {
                await FailSaga(context, "Users do not follow each other").ConfigureAwait(false);
                return;
            }

            await context.Send(new AddMessageRequest()
            {
                Message = Data.Message,
                CorrelationId = Data.CorrelationId,
                DateCreated = Data.DateCreated,
                ReceiverId = Data.ReceiverId,
                SenderId = Data.SenderId
            }).ConfigureAwait(false);
        }
        
        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }

        private async Task<string> Validate(BeginAddMessageRequest message)
        {
            var retVal = string.Empty;
            
            if (message.SenderId == Guid.Empty)
            {
                retVal += $"SenderId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.SenderId.ToString()) == null)
            {
                retVal += $"User with id {message.SenderId} not found\n";
            }

            if (message.ReceiverId == Guid.Empty)
            {
                retVal += $"ReceiverId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.ReceiverId.ToString()) == null)
            {
                retVal += $"User with id {message.ReceiverId} not found\n";
            }

            if (message.SenderId == message.ReceiverId)
            {
                retVal += $"User and sender cannot be the same person";
            }

            if (string.IsNullOrEmpty(message.Message))
            {
                retVal += $"Message cannot be empty\n";
            }

            return retVal;
        }

        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.SenderId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);

            MarkAsComplete();
        }
    }
}