using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.AnswerFollowSaga
{
    public class AnswerFollowSaga : Saga<AnswerFollowSagaData>,
        IAmStartedByMessages<BeginAnswerFollowRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<AnswerFollowResponse>
    {
        private readonly UserManager<User> _userManager;

        public AnswerFollowSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AnswerFollowSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginAnswerFollowRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<AnswerFollowResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginAnswerFollowRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.FollowerId = message.FollowerId;
            Data.ObservedId = message.ObservedId;
            Data.IsAccepted = message.IsAccepted;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout()
            {
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);

            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new AnswerFollowRequest()
            {
                CorrelationId = Data.CorrelationId,
                FollowerId = Data.FollowerId,
                ObservedId = Data.ObservedId,
                IsAccepted = message.IsAccepted
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }
        
        public async Task Handle(AnswerFollowResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = $"Successfully {(Data.IsAccepted ? "accepted!" : "rejected!")}",
                IsSuccessful = true,
                UserId = Data.ObservedId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);

            await context.SendLocal(new StandardNotification()
            {
                Message =
                    $"Follow request answered! User {Data.ObservedId} {(Data.IsAccepted ? "accepted" : "rejected")} the request!",
                IsSuccessful = true,
                UserId = Data.FollowerId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        private async Task<string> Validate(BeginAnswerFollowRequest message)
        {
            var retVal = string.Empty;

            if (message.FollowerId == Guid.Empty)
            {
                retVal += $"FollowerId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.FollowerId.ToString()) == null)
            {
                retVal += $"User with id {message.FollowerId} not found\n";
            }

            if (message.ObservedId == Guid.Empty)
            {
                retVal += $"ObservedId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.ObservedId.ToString()) == null)
            {
                retVal += $"User with id {message.ObservedId} not found\n";
            }
            
            return retVal;
        }
        
        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.ObservedId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}