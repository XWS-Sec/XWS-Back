using System;
using NServiceBus;

namespace BaseApi.Sagas.AnswerFollowSaga
{
    public class AnswerFollowSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }

        public Guid FollowerId { get; set; }
        public Guid ObservedId { get; set; }
        public bool IsAccepted { get; set; }
    }
}