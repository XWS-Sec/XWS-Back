using System.Collections.Generic;

namespace AgentApiModel
{
    public class JobOffer
    {
        public string JobTitle { get; set; }
        public string Description { get; set; }
        public string Prerequisites { get; set; }
        public IList<JobOfferComment> JobOfferComments { get; set; }
    }
}