using System;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class UnblockResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}