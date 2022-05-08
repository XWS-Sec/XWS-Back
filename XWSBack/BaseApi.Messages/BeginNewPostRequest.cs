using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginNewPostRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; }
        [CustomDataBus]
        public byte[] Picture { get; set; }
    }
}