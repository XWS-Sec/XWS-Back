using System;
using NServiceBus;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class AnswerFollowResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}