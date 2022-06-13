using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginBlockUnblockRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid OtherUserId { get; set; }
        public bool IsBlock { get; set; }
    }
}