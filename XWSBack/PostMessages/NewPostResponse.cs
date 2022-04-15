using System;
using NServiceBus;

namespace PostMessages
{
    public class NewPostResponse : IMessage
    {
        public Guid PostId { get; set; }
        public bool IsSuccessful { get; set; }
    }
}