﻿using System;
using Shared.Custom;

namespace JobOffers.Messages
{
    public class PublishNewJobOfferRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public string LinkToJobOffer { get; set; }
        public string Description { get; set; }
        public string JobTitle { get; set; }
        public string Prerequisites { get; set; }
        public string ApiKey { get; set; }
    }
}