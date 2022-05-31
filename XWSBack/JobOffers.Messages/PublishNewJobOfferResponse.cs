using System;
using Shared.Custom;

namespace JobOffers.Messages
{
    public class PublishNewJobOfferResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
    }
}