using System;
using System.Collections.Generic;
using NServiceBus;

namespace BaseApi.Sagas.AnswerAllFollowRequestsSaga
{
    public class AnswerAllFollowRequestsSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public int SentRequests { get; set; }
        public Guid User { get; set; }
    }
}