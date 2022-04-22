using System;
using NServiceBus;
using Shared.Custom;

namespace Posts.Messages
{
    public class EditPostResponse : ICustomMessage
    {
        public bool IsSuccessful { get; set; }
        public Guid CorrelationId { get; set; }
        public string MessageToLog { get; set; }

        public bool ShouldDeleteOldPicture { get; set; }
    }
}