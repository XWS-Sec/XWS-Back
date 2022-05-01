using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginAddMessageRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
    }
}