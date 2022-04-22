using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class GetFollowStatsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}