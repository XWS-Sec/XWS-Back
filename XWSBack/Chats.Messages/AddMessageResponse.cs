using System;
using Shared.Custom;

namespace Chats.Messages
{
    public class AddMessageResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}