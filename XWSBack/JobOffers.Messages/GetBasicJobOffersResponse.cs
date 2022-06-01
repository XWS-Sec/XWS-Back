using System;
using System.Collections.Generic;
using JobOffers.Messages.Dtos;
using Shared.Custom;

namespace JobOffers.Messages
{
    public class GetBasicJobOffersResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public IEnumerable<JobOfferDto> JobOffers { get; set; }
    }
}