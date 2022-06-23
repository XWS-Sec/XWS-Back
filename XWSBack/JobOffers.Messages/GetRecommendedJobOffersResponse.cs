using JobOffers.Messages.Dtos;
using Shared.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobOffers.Messages
{
    public class GetRecommendedJobOffersResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<JobOfferDto> JobOffers { get; set; }
    }
}
