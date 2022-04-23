using System;

namespace Posts.Messages.Dtos
{
    public class CommentDto
    {
        public Guid CommenterId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
    }
}