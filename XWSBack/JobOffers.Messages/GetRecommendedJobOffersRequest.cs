using Shared.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobOffers.Messages
{
    public class GetRecommendedJobOffersRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<string> Interests { get; set; }
    }
}
