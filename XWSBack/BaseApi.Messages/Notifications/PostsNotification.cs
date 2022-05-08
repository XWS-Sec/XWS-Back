using System;
using System.Collections.Generic;
using BaseApi.Messages.Dtos;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class PostsNotification : ICustomMessage
    {
        public Guid UserId { get; set; }
        public IEnumerable<PostNotificationDto> Posts { get; set; }
    }
}