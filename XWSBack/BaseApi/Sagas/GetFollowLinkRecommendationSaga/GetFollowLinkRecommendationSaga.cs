using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.GetFollowLinkRecommendationSaga
{
    public class GetFollowLinkRecommendationSaga : Saga<GetFollowLinkRecommendationSagaData>,
        IAmStartedByMessages<BeginGetLinkRecommendationsRequest>,
        IHandleMessages<RecommendNewLinksResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public GetFollowLinkRecommendationSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetFollowLinkRecommendationSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginGetLinkRecommendationsRequest>(m => m.CorrelationId)
                .ToMessage<RecommendNewLinksResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginGetLinkRecommendationsRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());

            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new RecommendNewLinksRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Handle(RecommendNewLinksResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = JsonConvert.SerializeObject(message.Recommended),
                CorrelationId = Data.CorrelationId,
                IsSuccessful = true,
                UserId = Guid.NewGuid()
            }).ConfigureAwait(false);
            MarkAsComplete();
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }

        private async Task<string> ValidateMessage(BeginGetLinkRecommendationsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += "User is mandatory";
            }
            else if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += "User does not exist";
            }
            
            return retVal;
        }

        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.UserId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}