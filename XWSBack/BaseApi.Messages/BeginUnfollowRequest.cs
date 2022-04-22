using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginUnfollowRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid Receiver { get; set; }
        public Guid Sender { get; set; }
    }
}