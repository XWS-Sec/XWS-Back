using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.AnswerAllFollowRequestsSaga
{
    public class AnswerAllFollowRequestsSaga : Saga<AnswerAllFollowRequestsSagaData>,
        IAmStartedByMessages<BeginAnswerAllFollowRequestsRequest>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleMessages<AnswerFollowResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public AnswerAllFollowRequestsSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AnswerAllFollowRequestsSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginAnswerAllFollowRequestsRequest>(m => m.CorrelationId)
                .ToMessage<GetFollowStatsResponse>(m => m.CorrelationId)
                .ToMessage<AnswerFollowResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginAnswerAllFollowRequestsRequest message, IMessageHandlerContext context)
        {
            Data.User = message.UserId;
            Data.CorrelationId = message.CorrelationId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());

            var validationStatus = await ValidateMessage(message);
            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.User
            }).ConfigureAwait(false);
        }
        
        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            foreach (var userToAnswer in message.FollowRequests)
            {
                await context.Send(new AnswerFollowRequest()
                {
                    CorrelationId = Data.CorrelationId,
                    IsAccepted = true,
                    ObservedId = Data.User,
                    FollowerId = userToAnswer
                }).ConfigureAwait(false);
            }
        }

        public async Task Handle(AnswerFollowResponse message, IMessageHandlerContext context)
        {
            Data.SentRequests--;

            if (Data.SentRequests == 0)
            {
                await context.SendLocal(new StandardNotification()
                {
                    Message = "Successfully answered",
                    CorrelationId = Data.CorrelationId,
                    IsSuccessful = true,
                }).ConfigureAwait(false);
                
                MarkAsComplete();
            }
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }

        private async Task<string> ValidateMessage(BeginAnswerAllFollowRequestsRequest request)
        {
            var retVal = string.Empty;

            if (request.UserId == Guid.Empty)
            {
                retVal += "User is mandatory";
            }
            else
            {
                if (await _userManager.FindByIdAsync(request.UserId.ToString()) == null)
                {
                    retVal += "User with that id not found";
                }
            }
            
            return retVal;
        }
        
        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.User,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}