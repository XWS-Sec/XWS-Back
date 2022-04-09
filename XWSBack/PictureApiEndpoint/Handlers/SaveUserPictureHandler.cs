using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using PictureApiEndpoint.Extensions;
using PictureMessages;

namespace PictureApiEndpoint.Handlers
{
    public class SaveUserPictureHandler : IHandleMessages<SaveUserPicture>
    {
        private readonly ILogger<SaveUserPictureHandler> _logger;

        public SaveUserPictureHandler(ILogger<SaveUserPictureHandler> logger)
        {
            _logger = logger;
        }
        
        public Task Handle(SaveUserPicture message, IMessageHandlerContext context)
        {
            var convertedImage = message.Image.GetBytes();
            var fileExtension = convertedImage.GetImageFormat();

            if (!string.IsNullOrEmpty(fileExtension))
            {
                var userPics = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
                userPics = Environment.ExpandEnvironmentVariables(userPics);
                File.WriteAllBytes($"{userPics}\\{message.UserId}", convertedImage);
                _logger.Log(LogLevel.Information, $"Profile picture added for user {message.UserId}");
            }
            else
            {
                _logger.Log(LogLevel.Warning,$"User with id {message.Image} sent an invalid image format");
            }
            return Task.CompletedTask;
        }
    }
}