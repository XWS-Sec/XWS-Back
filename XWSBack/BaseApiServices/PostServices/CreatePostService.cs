using System;
using System.Threading.Tasks;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using PostMessages;
using Services.PictureServices;

namespace Services.PostServices
{
    public class CreatePostService
    {
        private readonly PictureService _pictureService;
        private readonly IMessageSession _messageSession;

        public CreatePostService(UserManager<User> userManager, PictureService pictureService, IMessageSession messageSession)
        {
            _pictureService = pictureService;
            _messageSession = messageSession;
        }

        public async Task<Guid> Create(User user, string text, byte[] picture)
        {
            var newPostId = Guid.NewGuid();
            var hasPicture = _pictureService.SavePostPicture(newPostId, picture);

            var request = new NewPostRequest()
            {
                PostId = newPostId,
                UserId = user.Id,
                Text = text,
                HasPicture = hasPicture
            };

            await _messageSession.Send(request).ConfigureAwait(false);
            return newPostId;
        }
    }
}