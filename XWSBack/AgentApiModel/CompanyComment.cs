using System;

namespace AgentApiModel
{
    public class CompanyComment
    {
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}