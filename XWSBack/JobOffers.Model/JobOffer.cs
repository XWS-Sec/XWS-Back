using System;

namespace JobOffers.Model
{
    public class JobOffer
    {
        public Guid Id { get; set; }
        public Guid ExternalCorrelation { get; set; }
        public string Description { get; set; }
        public string JobTitle { get; set; }
        public string Prerequisites { get; set; }
    }
}