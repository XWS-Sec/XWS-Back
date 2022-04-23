using System;
using System.Collections.Generic;
using Shared.Custom;

namespace Posts.Messages
{
    public class GetPostsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<Guid> PostsOwners { get; set; }
        public int Page { get; set; }
    }
}