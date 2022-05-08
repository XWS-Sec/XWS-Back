using System;
using NServiceBus;
using Shared.Custom;

namespace BaseApi.Messages.Timeouts
{
    public class BaseTimeout : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
    }
}