using System;
using NServiceBus;

namespace PictureMessages
{
    public class SaveUserPicture : ICommand
    {
        public string Image { get; set; }
        public Guid UserId { get; set; }
    }
}