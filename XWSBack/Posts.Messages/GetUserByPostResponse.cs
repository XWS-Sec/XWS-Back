using System;
using Shared.Custom;

namespace Posts.Messages
{
    public class GetUserByPostResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public Guid PostOwnerId { get; set; }
        public bool IsSuccessful { get; set; }
    }
}