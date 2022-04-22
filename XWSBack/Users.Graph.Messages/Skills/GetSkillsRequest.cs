using System;
using Shared.Custom;

namespace Users.Graph.Messages.Skills
{
    public class GetSkillsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string LinkName { get; set; }
    }
}