using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class UnfollowResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public string MessageToLog { get; set; }
        public bool IsSuccessful { get; set; }
    }
}