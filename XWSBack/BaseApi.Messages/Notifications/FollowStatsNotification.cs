using System;
using System.Collections.Generic;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class FollowStatsNotification : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<Guid> Following { get; set; }
        public IEnumerable<Guid> Followers { get; set; }
        public IEnumerable<Guid> FollowRequests { get; set; }
    }
}