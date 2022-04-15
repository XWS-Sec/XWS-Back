using System.Threading.Tasks;
using NServiceBus;
using PostMessages;
using Services.PictureServices;

namespace Services.PostServices
{
    public class EditPostResponseHandler : IHandleMessages<EditPostResponse>
    {
        private readonly PictureService _pictureService;

        public EditPostResponseHandler(PictureService pictureService)
        {
            _pictureService = pictureService;
        }
        
        public Task Handle(EditPostResponse message, IMessageHandlerContext context)
        {
            if (message.IsSuccessful)
            {
                _pictureService.DeletePostPicture(message.PostId);
                _pictureService.ChangePostPictureName(message.TempPicId, message.PostId);
            }
            else
            {
                _pictureService.DeletePostPicture(message.TempPicId);
            }

            return Task.CompletedTask;
        }
    }
}