using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class UnblockRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid RequesterId { get; set; }
        public Guid BlockedUserId { get; set; }
    }
}