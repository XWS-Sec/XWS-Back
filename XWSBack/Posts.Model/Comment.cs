using System;

namespace Posts.Model
{
    public class Comment
    {
        public Guid CommenterId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}