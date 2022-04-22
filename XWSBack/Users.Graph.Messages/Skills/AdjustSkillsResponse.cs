using System;
using Shared.Custom;

namespace Users.Graph.Messages.Skills
{
    public class AdjustSkillsResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}