using System;
using MongoDB.Bson.Serialization.Attributes;

namespace BaseApi.Dto.Users
{
    [BsonIgnoreExtraElements]
    public class SearchedUserDto
    {
        public Guid Id { get; set; }

        [BsonElement("UserName")] public string Username { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsPrivate { get; set; }
    }
}