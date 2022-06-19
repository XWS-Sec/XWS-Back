using System;
using NServiceBus;

namespace BaseApi.Sagas.GetBasicJobOffersSaga
{
    public class GetBasicJobOffersSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
    }
}