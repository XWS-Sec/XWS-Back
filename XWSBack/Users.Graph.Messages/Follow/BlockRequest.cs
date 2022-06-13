using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class BlockRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid RequesterId { get; set; }
        public Guid UserToBlockId { get; set; }
    }
}