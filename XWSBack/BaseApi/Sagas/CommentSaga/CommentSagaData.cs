using System;
using NServiceBus;

namespace BaseApi.Sagas.CommentSaga
{
    public class CommentSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public string Text { get; set; }
        public Guid PostOwnerId { get; set; }
    }
}