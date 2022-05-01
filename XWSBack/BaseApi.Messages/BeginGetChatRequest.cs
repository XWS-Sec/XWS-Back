using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetChatRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid OtherUserId { get; set; }
        public int Page { get; set; }
    }
}