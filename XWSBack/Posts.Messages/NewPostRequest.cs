using System;
using NServiceBus;
using Shared.Custom;

namespace Posts.Messages
{
    public class NewPostRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid PostId { get; set; }
        public string Text { get; set; }
        public Guid UserId { get; set; }
        public bool HasPicture { get; set; }
    }
}