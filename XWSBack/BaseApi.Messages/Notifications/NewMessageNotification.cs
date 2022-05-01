using System;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class NewMessageNotification : ICustomMessage
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
    }
}