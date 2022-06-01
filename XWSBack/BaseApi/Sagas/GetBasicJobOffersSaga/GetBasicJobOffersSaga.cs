using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BaseApi.Messages;
using BaseApi.Messages.Dtos;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using JobOffers.Messages;
using NServiceBus;

namespace BaseApi.Sagas.GetBasicJobOffersSaga
{
    public class GetBasicJobOffersSaga : Saga<GetBasicJobOffersSagaData>,
        IAmStartedByMessages<BeginGetBasicJobOffersRequest>,
        IHandleMessages<GetBasicJobOffersResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly IMapper _mapper;

        public GetBasicJobOffersSaga(IMapper mapper)
        {
            _mapper = mapper;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetBasicJobOffersSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginGetBasicJobOffersRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetBasicJobOffersResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginGetBasicJobOffersRequest message, IMessageHandlerContext context)
        {
            await context.Send(new GetBasicJobOffersRequest()
            {
                CorrelationId = message.CorrelationId
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetBasicJobOffersResponse message, IMessageHandlerContext context)
        {
            await context.SendLocal(new GetJobOffersNotification()
            {
                CorrelationId = Data.CorrelationId,
                JobOffers = _mapper.Map<IEnumerable<JobOfferNotificationDto>>(message.JobOffers)
            }).ConfigureAwait(false);
            MarkAsComplete();
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
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