using System;

namespace BaseApiModel.Mongo
{
    public class Experience
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
    }
}