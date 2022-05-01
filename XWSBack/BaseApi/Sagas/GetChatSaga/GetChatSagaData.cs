using System;
using NServiceBus;

namespace BaseApi.Sagas.GetChatSaga
{
    public class GetChatSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid OtherUserId { get; set; }
        public int Page { get; set; }
    }
}