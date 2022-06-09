using System;
using Shared.Custom;

namespace Posts.Messages
{
    public class LikeDislikeResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}