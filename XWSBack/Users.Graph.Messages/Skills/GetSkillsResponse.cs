using System;
using System.Collections.Generic;
using Shared.Custom;

namespace Users.Graph.Messages.Skills
{
    public class GetSkillsResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        public IEnumerable<string> Links { get; set; }
    }
}