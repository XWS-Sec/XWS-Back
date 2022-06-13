using System;
using System.Collections.Generic;
using Shared.Custom;

namespace Users.Graph.Messages.Follow
{
    public class RecommendNewLinksResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<Guid> Recommended { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}