using System;
using NServiceBus;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class CreateFollowLinkRequest : ICustomCommand
    {
        public Guid Sender { get; set; }
        public Guid Receiver { get; set; }
        public bool IsReceiverPrivate { get; set; }
        public Guid CorrelationId { get; set; }
    }
}