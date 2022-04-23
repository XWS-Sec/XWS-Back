using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetPostsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid RequestedUserId { get; set; }
        public int Page { get; set; }
    }
}