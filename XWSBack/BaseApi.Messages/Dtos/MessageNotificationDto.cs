using System;

namespace BaseApi.Messages.Dtos
{
    public class MessageNotificationDto
    {
        public Guid SenderId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Text { get; set; }
    }
}