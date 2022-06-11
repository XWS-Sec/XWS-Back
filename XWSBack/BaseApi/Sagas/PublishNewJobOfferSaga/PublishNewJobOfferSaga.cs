using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using JobOffers.Messages;
using NServiceBus;

namespace BaseApi.Sagas.PublishNewJobOfferSaga
{
    public class PublishNewJobOfferSaga : Saga<PublishNewJobOfferSagaData>,
        IAmStartedByMessages<BeginPublishNewJobOffer>,
        IHandleMessages<PublishNewJobOfferResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PublishNewJobOfferSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginPublishNewJobOffer>(m => m.CorrelationId)
                .ToMessage<PublishNewJobOfferResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginPublishNewJobOffer message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());
            
            var validationStatus = Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new PublishNewJobOfferRequest()
            {
                CorrelationId = message.CorrelationId,
                Description = message.Description,
                Prerequisites = message.Prerequisites,
                ApiKey = message.ApiKey,
                JobTitle = message.JobTitle,
                LinkToJobOffer = message.LinkToJobOffer
            }).ConfigureAwait(false);
        }

        public async Task Handle(PublishNewJobOfferResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = message.MessageToLog,
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

        private static string Validate(BeginPublishNewJobOffer message)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(message.Description))
                retVal += "Description is mandatory\n";

            if (string.IsNullOrEmpty(message.Prerequisites))
                retVal += "Prerequisites are required\n";

            if (string.IsNullOrEmpty(message.ApiKey))
                retVal += "ApiKey is mandatory\n";

            if (string.IsNullOrEmpty(message.JobTitle))
                retVal += "JobTitle is mandatory\n";

            if (string.IsNullOrEmpty(message.LinkToJobOffer))
                retVal += "Link to job offer is mandatory\n";
            
            return retVal;
        }
        
        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Guid.NewGuid(),
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}