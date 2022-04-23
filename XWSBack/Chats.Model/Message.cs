using System;

namespace Chats.Model
{
    public class Message
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
    }
}