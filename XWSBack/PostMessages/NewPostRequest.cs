using System;
using NServiceBus;

namespace PostMessages
{
    public class NewPostRequest : ICommand
    {
        public Guid PostId { get; set; }
        public string Text { get; set; }
        public Guid UserId { get; set; }
        public bool HasPicture { get; set; }
    }
}