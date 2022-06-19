using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginCommentRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public string Text { get; set; }
    }
}