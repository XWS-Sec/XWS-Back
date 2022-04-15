using System;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using NServiceBus;
using PostMessages;
using Services.PictureServices;

namespace Services.PostServices
{
    public class EditPostService
    {
        private readonly IMessageSession _messageSession;
        private readonly PictureService _pictureService;

        public EditPostService(IMessageSession messageSession, PictureService pictureService)
        {
            _messageSession = messageSession;
            _pictureService = pictureService;
        }

        public async Task Edit(Guid userId, Guid postId, string text, byte[] picture, bool removedPicture)
        {
            var tempPicId = Guid.NewGuid();
            _pictureService.SavePostPicture(tempPicId, picture);

            var request = new EditPostRequest()
            {
                Text = text,
                HasPicture = !removedPicture,
                PostId = postId,
                UserId = userId,
                TempPicId = tempPicId
            };

            await _messageSession.Send(request).ConfigureAwait(false);
        }
    }
}