using System;
using System.Collections.Generic;
using Chats.Messages.Dtos;
using Shared.Custom;

namespace Chats.Messages
{
    public class GetChatResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }

        public IEnumerable<MessageDto> Messages { get; set; }
    }
}