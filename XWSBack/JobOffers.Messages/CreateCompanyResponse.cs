using System;
using Shared.Custom;

namespace JobOffers.Messages
{
    public class CreateCompanyResponse : ICustomMessage
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccessful { get; set; }
        public string MessageToLog { get; set; }
        public string GeneratedApiKey { get; set; }
    }
}