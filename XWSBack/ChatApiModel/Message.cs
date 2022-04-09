using System;

namespace ChatApiModel
{
    public class Message
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}