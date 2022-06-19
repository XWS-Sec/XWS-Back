using System;
using Shared.Custom;

namespace Posts.Messages
{
    public class GetUserByPostRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid PostId { get; set; }
    }
}