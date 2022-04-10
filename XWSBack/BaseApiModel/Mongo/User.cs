using System;
using System.Collections.Generic;
using AspNetCore.Identity.MongoDbCore.Models;

namespace BaseApiModel.Mongo
{
    public class User : MongoIdentityUser<Guid>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Biography { get; set; }
        public bool IsPrivate { get; set; }
        public string Gender { get; set; }

        public IList<Experience> Experiences { get; set; }
        public NotificationConfiguration NotificationConfiguration { get; set; }
        public IList<Guid> BlockedUsers { get; set; }
    }
}