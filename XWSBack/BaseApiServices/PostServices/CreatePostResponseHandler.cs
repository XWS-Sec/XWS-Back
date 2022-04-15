using System.Threading.Tasks;
using NServiceBus;
using PostMessages;
using Services.PictureServices;

namespace Services.PostServices
{
    public class CreatePostResponseHandler : IHandleMessages<NewPostResponse>
    {
        private readonly PictureService _pictureService;

        public CreatePostResponseHandler(PictureService pictureService)
        {
            _pictureService = pictureService;
        }
        
        public Task Handle(NewPostResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                _pictureService.DeletePostPicture(message.PostId);
            }
            
            return Task.CompletedTask;
        }
    }
}