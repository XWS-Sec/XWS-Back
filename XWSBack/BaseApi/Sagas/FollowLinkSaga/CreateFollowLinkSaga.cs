using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.FollowLinkSaga
{
    public class CreateFollowLinkSaga : Saga<CreateFollowLinkSagaData>,
        IAmStartedByMessages<BeginFollowLinkRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<CreateFollowLinkResponse>
    {
        private readonly UserManager<User> _userManager;

        public CreateFollowLinkSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CreateFollowLinkSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginFollowLinkRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<CreateFollowLinkResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginFollowLinkRequest message, IMessageHandlerContext context)
        {
            Data.Receiver = message.Receiver;
            Data.Sender = message.Sender;
            Data.CorrelationId = message.CorrelationId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout()
            {
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);

            var validation = await Validate(message);
            if (!string.IsNullOrEmpty(validation))
            {
                await FailSaga(context, validation).ConfigureAwait(false);
                return;
            }

            var receiver = await _userManager.FindByIdAsync(Data.Receiver.ToString());
            Data.IsReceiverPrivate = receiver.IsPrivate;
            var request = new CreateFollowLinkRequest()
            {
                Sender = Data.Sender,
                Receiver = Data.Receiver,
                CorrelationId = Data.CorrelationId,
                IsReceiverPrivate = Data.IsReceiverPrivate
            };

            await context.Send(request).ConfigureAwait(false);
        }
        
        public async Task Handle(CreateFollowLinkResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = "Successful!",
                IsSuccessful = true,
                UserId = Data.Sender,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);

            await context.SendLocal(new StandardNotification()
            {
                Message = Data.IsReceiverPrivate ? "New follow request!" : "New follower!",
                IsSuccessful = true,
                UserId = Data.Receiver
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        private async Task<string> Validate(BeginFollowLinkRequest message)
        {
            var retVal = string.Empty;

            if (message.Sender == message.Receiver)
            {
                retVal += $"User cannot follow himself\n";
            }
            
            var sender = await _userManager.FindByIdAsync(message.Sender.ToString());
            if (sender == null)
            {
                retVal += $"User with id {message.Sender} doesn't exist\n";
            }
            
            var receiver = await _userManager.FindByIdAsync(message.Receiver.ToString());
            if (receiver == null)
            {
                retVal += $"User with id {message.Receiver} doesn't exist";
            }

            return retVal;
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
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