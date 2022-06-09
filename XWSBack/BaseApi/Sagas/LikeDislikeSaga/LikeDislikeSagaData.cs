using System;
using MongoDB.Bson;
using NServiceBus;

namespace BaseApi.Sagas.LikeDislikeSaga
{
    public class LikeDislikeSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public Guid PostOwnerId { get; set; }
        public bool IsLike { get; set; }
    }
}