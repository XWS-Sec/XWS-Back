using System;
using NServiceBus;

namespace BaseApi.Sagas.GetPostsSaga
{
    public class GetPostsSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid RequestedUserId { get; set; }
        public int Page { get; set; }
    }
}