using System;
using System.Collections.Generic;
using AspNetCore.Identity.MongoDbCore.Models;

namespace AgentApiModel
{
    public class AgentUser : MongoIdentityUser<Guid>
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public IList<Company> Companies { get; set; }
    }
}