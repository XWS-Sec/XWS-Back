using System;
using System.Collections.Generic;
using Shared.Custom;

namespace BaseApi.Messages.Notifications
{
    public class SkillStatsNotification : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        public IEnumerable<string> Skills { get; set; }
        public string LinkName { get; set; }
        public Guid UserId { get; set; }
    }
}