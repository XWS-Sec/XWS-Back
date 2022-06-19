using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetLinkRecommendationsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}