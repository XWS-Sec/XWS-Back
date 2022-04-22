using System;
using System.Collections.Generic;
using Shared.Custom;

namespace Users.Graph.Messages.Skills
{
    public class AdjustSkillsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<string> NewSkills { get; set; }
        public IEnumerable<string> SkillsToRemove { get; set; }
        public string LinkName { get; set; }
    }
}