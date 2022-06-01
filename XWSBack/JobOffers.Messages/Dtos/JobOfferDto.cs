using System;

namespace JobOffers.Messages.Dtos
{
    public class JobOfferDto
    {
        public Guid Id { get; set; }
        public string LinkToJobOffer { get; set; }
        public string Description { get; set; }
        public string JobTitle { get; set; }
        public string Prerequisites { get; set; }
    }
}