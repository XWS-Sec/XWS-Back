using System;
using NServiceBus;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class StandardNotification : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
    }
}