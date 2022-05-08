using System;
using NServiceBus;

namespace BaseApi.Sagas.FollowLinkSaga
{
    public class CreateFollowLinkSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid Sender { get; set; }
        public Guid Receiver { get; set; }
        public bool IsReceiverPrivate { get; set; }
    }
}