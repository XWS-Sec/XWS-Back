using System;

namespace AgentApiModel
{
    public class JobOfferComment
    {
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public string Salary { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}