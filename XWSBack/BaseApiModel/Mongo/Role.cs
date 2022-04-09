using System;
using AspNetCore.Identity.MongoDbCore.Models;

namespace BaseApiModel.Mongo
{
    public class Role : MongoIdentityRole<Guid>
    {
    }
}