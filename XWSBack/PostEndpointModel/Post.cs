using System;
using System.Collections.Generic;

namespace PostServiceModel
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public string PictureLocation { get; set; }
        public IList<Guid> Liked { get; set; }
        public IList<Guid> Disliked { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}