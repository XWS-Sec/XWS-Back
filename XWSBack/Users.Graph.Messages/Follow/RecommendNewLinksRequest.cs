using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class RecommendNewLinksRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}