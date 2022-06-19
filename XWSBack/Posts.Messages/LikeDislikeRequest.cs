using System;
using Shared.Custom;

namespace Posts.Messages
{
    public class LikeDislikeRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public bool IsLike { get; set; }
    }
}