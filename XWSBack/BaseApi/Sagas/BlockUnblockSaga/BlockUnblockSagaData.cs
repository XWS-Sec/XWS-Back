using System;
using NServiceBus;

namespace BaseApi.Sagas.BlockUnblockSaga
{
    public class BlockUnblockSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid OtherUserId { get; set; }
    }
}