using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class UnfollowRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid Sender { get; set; }
        public Guid Receiver { get; set; }
    }
}