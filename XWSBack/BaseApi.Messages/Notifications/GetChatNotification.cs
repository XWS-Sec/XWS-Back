using System;
using System.Collections.Generic;
using BaseApi.Messages.Dtos;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class GetChatNotification : ICustomMessage
    {
        public Guid UserId { get; set; }
        public Guid OtherUserId { get; set; }
        public IEnumerable<MessageNotificationDto> Messages { get; set; }
    }
}