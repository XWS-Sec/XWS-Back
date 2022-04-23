using System;

namespace BaseApi.Model.Mongo
{
    public class Milestone
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Title { get; set; }
        public string OrganizationName { get; set; }
        public string Description { get; set; }
    }
}