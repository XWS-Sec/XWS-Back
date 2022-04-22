using System;
using NServiceBus;

namespace BaseApi.Sagas.GetFollowStatsSaga
{
    public class GetFollowStatsSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }

        public Guid UserId { get; set; }
    }
}