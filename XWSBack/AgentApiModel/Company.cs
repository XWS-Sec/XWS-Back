using System.Collections.Generic;

namespace AgentApiModel
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }

        public IList<CompanyComment> CompanyComments { get; set; }
        public IList<JobOffer> JobOffers { get; set; }
    }
}