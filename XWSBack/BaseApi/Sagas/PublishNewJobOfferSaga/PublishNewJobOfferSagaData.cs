using System;
using NServiceBus;

namespace BaseApi.Sagas.PublishNewJobOfferSaga
{
    public class PublishNewJobOfferSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
    }
}