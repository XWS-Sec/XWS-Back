using System;
using Shared.Custom;

namespace Posts.Messages
{
    public class CommentRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }

        public Guid PostId { get; set; }
        public Guid UserId { get; set; }

        public string Text { get; set; }
    }
}