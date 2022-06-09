using System;
using Posts.Messages.Dtos;
using Shared.Custom;

namespace Posts.Messages
{
    public class CommentResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public PostDto EditedPost { get; set; }
    }
}