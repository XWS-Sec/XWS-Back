using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetSkillsRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string LinkName { get; set; }
    }
}