using System;
using Shared.Custom;

namespace JobOffers.Messages
{
    public class GetBasicJobOffersRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
    }
}