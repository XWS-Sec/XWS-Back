using System;
using NServiceBus;

namespace PostMessages
{
    public class EditPostResponse : IMessage
    {
        public bool IsSuccessful { get; set; }
        public bool ShouldChangePic { get; set; }
        public Guid TempPicId { get; set; }
        public Guid PostId { get; set; }
    }
}