using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.UnfollowSaga
{
    public class UnfollowSaga : Saga<UnfollowSagaData>,
        IAmStartedByMessages<BeginUnfollowRequest>,
        IHandleMessages<UnfollowResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public UnfollowSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<UnfollowSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginUnfollowRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<UnfollowResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginUnfollowRequest message, IMessageHandlerContext context)
        {
            Data.Receiver = message.Receiver;
            Data.Sender = message.Sender;
            Data.CorrelationId = message.CorrelationId;

            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            await context.Send(new UnfollowRequest()
            {
                Receiver = Data.Receiver,
                Sender = Data.Sender,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
        }
        
        public async Task Handle(UnfollowResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                IsSuccessful = true,
                Message = $"Successfully unfollowed user {Data.Receiver}",
                UserId = Data.Sender,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }
        
        private async Task<string> Validate(BeginUnfollowRequest message)
        {
            var retVal = string.Empty;

            if (message.Sender == Guid.Empty)
            {
                retVal += $"Sender is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.Sender.ToString()) == null)
            {
                retVal += $"User with id {message.Sender} not found\n";
            }

            if (message.Receiver == Guid.Empty)
            {
                retVal += $"Receiver is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.Receiver.ToString()) == null)
            {
                retVal += $"User with id {message.Receiver} not found\n";
            }

            return retVal;
        }

        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.Sender,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}