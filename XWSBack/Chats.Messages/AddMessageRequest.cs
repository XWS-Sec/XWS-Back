using System;
using Shared.Custom;

namespace Chats.Messages
{
    public class AddMessageRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public DateTime DateCreated { get; set; }

        public string Message { get; set; }
    }
}