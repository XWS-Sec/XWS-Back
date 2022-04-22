using System;
using NServiceBus;

namespace BaseApi.Sagas.UnfollowSaga
{
    public class UnfollowSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid Receiver { get; set; }
        public Guid Sender { get; set; }
    }
}