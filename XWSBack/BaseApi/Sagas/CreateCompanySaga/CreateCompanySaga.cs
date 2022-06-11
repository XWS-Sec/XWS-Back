using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using JobOffers.Messages;
using NServiceBus;

namespace BaseApi.Sagas.CreateCompanySaga
{
    public class CreateCompanySaga : Saga<CreateCompanySagaData>,
        IAmStartedByMessages<BeginCreateCompanyRequest>,
        IHandleMessages<CreateCompanyResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CreateCompanySagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginCreateCompanyRequest>(m => m.CorrelationId)
                .ToMessage<CreateCompanyResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginCreateCompanyRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());
            
            var validationStatus = Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            await context.Send(new CreateCompanyRequest()
            {
                Email = message.Email,
                Name = message.Name,
                CorrelationId = Data.CorrelationId,
                PhoneNumber = message.PhoneNumber,
                LinkToCompany = message.LinkToCompany
            }).ConfigureAwait(false);
        }

        public async Task Handle(CreateCompanyResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = message.GeneratedApiKey,
                CorrelationId = Data.CorrelationId,
                IsSuccessful = true,
                UserId = Guid.NewGuid()
            }).ConfigureAwait(false);
            MarkAsComplete();
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
                UserId = Guid.NewGuid(),
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }

        private static string Validate(BeginCreateCompanyRequest message)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(message.Email))
                retVal += "Email is mandatory\n";

            if (string.IsNullOrEmpty(message.Name))
                retVal += "Name is mandatory\n";

            if (string.IsNullOrEmpty(message.PhoneNumber))
                retVal += "Phone number of company is mandatory\n";

            if (string.IsNullOrEmpty(message.LinkToCompany))
                retVal += "Link to company is mandatory\n";

            return retVal;
        }
    }
}