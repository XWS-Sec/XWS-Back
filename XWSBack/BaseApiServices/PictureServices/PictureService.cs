using System;
using System.IO;
using System.Threading.Tasks;
using BaseApiService.Extensions;
using Microsoft.Extensions.Logging;

namespace Services.PictureServices
{
    public class PictureService
    {
        private readonly ILogger<PictureService> _logger;

        public PictureService(ILogger<PictureService> logger)
        {
            _logger = logger;
        }

        public void SavePicture(Guid userId, byte[] picture)
        {
            if (picture == null || picture.Length == 0)
                return;
            
            var fileExtension = picture.GetImageFormat();

            if (!string.IsNullOrEmpty(fileExtension))
            {
                var userPics = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
                userPics = Environment.ExpandEnvironmentVariables(userPics);
                File.WriteAllBytes($"{userPics}\\{userId}", picture);
                _logger.Log(LogLevel.Information, $"Profile picture added for user {userId}");
            }
            else
            {
                _logger.Log(LogLevel.Warning,$"User with id {userId} sent an invalid image format");
            }
        }
    }
}