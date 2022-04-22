using System;
using NServiceBus;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class AnswerFollowRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }

        public Guid ObservedId { get; set; }
        public Guid FollowerId { get; set; }

        public bool IsAccepted { get; set; }
    }
}