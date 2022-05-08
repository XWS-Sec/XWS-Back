using System;

namespace BaseApi.Messages.Dtos
{
    public class CommentNotificationDto
    {
        public Guid CommenterId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
    }
}