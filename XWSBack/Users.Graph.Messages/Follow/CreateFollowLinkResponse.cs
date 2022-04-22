using System;
using NServiceBus;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class CreateFollowLinkResponse : ICustomMessage
    {
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        public Guid CorrelationId { get; set; }
    }
}