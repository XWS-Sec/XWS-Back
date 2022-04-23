using System;
using System.Collections.Generic;

namespace BaseApi.Messages.Dtos
{
    public class PostNotificationDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid PosterId { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<Guid> Liked { get; set; }
        public IEnumerable<Guid> Disliked { get; set; }
        public IEnumerable<CommentNotificationDto> Comments { get; set; }
    }
}