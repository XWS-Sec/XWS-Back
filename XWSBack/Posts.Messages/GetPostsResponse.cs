using System;
using System.Collections.Generic;
using Posts.Messages.Dtos;
using Shared.Custom;

namespace Posts.Messages
{
    public class GetPostsResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }

        public IEnumerable<PostDto> Posts { get; set; }
    }
}