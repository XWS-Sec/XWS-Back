using System;
using System.Collections.Generic;

namespace JobOffers.Model
{
    public class Company
    {
        public Guid InternalId { get; set; }
        public Guid ExternalCorrelation { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string ApiKeyHash { get; set; }

        public IList<JobOffer> JobOffers { get; set; }
    }
}