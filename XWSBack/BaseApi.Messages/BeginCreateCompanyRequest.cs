using System;
using Shared.Custom;

namespace BaseApi.Messages
{
    public class BeginCreateCompanyRequest : ICustomCommand
    {
        public Guid CorrelationId { get; set; }
        public string LinkToCompany { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}