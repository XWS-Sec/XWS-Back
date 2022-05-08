using System;
using NServiceBus;
using Shared.Custom;

namespace Posts.Messages
{
    public class EditPostRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public bool HasPicture { get; set; }
        public string Text { get; set; }
        public bool RemoveOldPic { get; set; }
    }
}