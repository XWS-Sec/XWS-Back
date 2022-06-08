using System;
using NServiceBus;
using Posts.Messages.Dtos;
using Shared.Custom;

namespace Posts.Messages
{
    public class NewPostResponse : ICustomMessage
    {
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        public Guid CorrelationId { get; set; }
        public PostDto Post { get; set; }
    }
}