using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginEditPostRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        [CustomDataBus]
        public byte[] Picture { get; set; }

        public string Text { get; set; }
        public bool RemoveOldPic { get; set; }
    }
}