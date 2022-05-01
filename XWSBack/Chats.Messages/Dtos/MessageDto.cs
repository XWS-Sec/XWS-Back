using System;

namespace Chats.Messages.Dtos
{
    public class MessageDto
    {
        public Guid SenderId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Text { get; set; }
    }
}