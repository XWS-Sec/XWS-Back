using System;
using System.Collections.Generic;

namespace Posts.Messages.Dtos
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<Guid> Liked { get; set; }
        public IEnumerable<Guid> Disliked { get; set; }
        public IEnumerable<CommentDto> Comments { get; set; }
    }
}