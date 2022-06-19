using System;
using NServiceBus;

namespace BaseApi.Sagas.GetFollowLinkRecommendationSaga
{
    public class GetFollowLinkRecommendationSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}