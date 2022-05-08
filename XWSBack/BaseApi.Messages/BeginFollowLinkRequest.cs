using System;
using NServiceBus;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginFollowLinkRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid Sender { get; set; }
        public Guid Receiver { get; set; }
    }
}