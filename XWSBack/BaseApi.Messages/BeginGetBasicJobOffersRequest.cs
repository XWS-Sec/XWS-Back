using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginGetBasicJobOffersRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
    }
}