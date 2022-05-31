using System;
using System.Collections.Generic;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class NewPostNotification : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<Guid> UsersToNotify { get; set; }
        public Guid Poster { get; set; }
    }
}