using System;
using System.Collections.Generic;
using BaseApi.Messages.Dtos;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class FollowStatsNotification : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<UserNotificationDto> Following { get; set; }
        public IEnumerable<UserNotificationDto> Followers { get; set; }
        public IEnumerable<UserNotificationDto> FollowRequests { get; set; }
        public IEnumerable<UserNotificationDto> FollowRequested { get; set; }
    }
}