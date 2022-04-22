using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetFollowStatsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}