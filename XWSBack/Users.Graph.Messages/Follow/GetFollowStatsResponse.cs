using System;
using System.Collections.Generic;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class GetFollowStatsResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }

        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        
        public IEnumerable<Guid> Following { get; set; }
        public IEnumerable<Guid> Followers { get; set; }
        public IEnumerable<Guid> FollowRequests { get; set; }
        public IEnumerable<Guid> FollowRequested { get; set; }
    }
}