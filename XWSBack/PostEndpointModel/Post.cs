using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace PostServiceModel
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public bool HasPicture { get; set; }

        public IList<Guid> Liked { get; set; }
        public IList<Guid> Disliked { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}