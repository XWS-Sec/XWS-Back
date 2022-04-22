using System;
using NServiceBus;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginAnswerFollowRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid ObservedId { get; set; }
        public Guid FollowerId { get; set; }
        public bool IsAccepted { get; set; }
    }
}