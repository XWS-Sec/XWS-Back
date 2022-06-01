using System;

namespace BaseApi.Messages.Dtos
{
    public class JobOfferNotificationDto
    {
        public Guid Id { get; set; }
        public string LinkToJobOffer { get; set; }
        public string Description { get; set; }
        public string JobTitle { get; set; }
        public string Prerequisites { get; set; }
    }
}