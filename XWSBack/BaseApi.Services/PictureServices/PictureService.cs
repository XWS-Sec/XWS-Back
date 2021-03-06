using System;
using System.IO;
using BaseApi.Services.Extensions;
using Microsoft.Extensions.Logging;

namespace BaseApi.Services.PictureServices
{
    public class PictureService
    {
        private readonly ILogger<PictureService> _logger;

        public PictureService(ILogger<PictureService> logger)
        {
            _logger = logger;
        }

        public void SaveUserPicture(Guid userId, byte[] picture)
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
                _logger.Log(LogLevel.Warning, $"User with id {userId} sent an invalid image format");
            }
        }

        public void DeleteUserPicture(Guid userId)
        {
            var userPics = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
            userPics = Environment.ExpandEnvironmentVariables(userPics);
            if (File.Exists($"{userPics}\\{userId}")) File.Delete($"{userPics}\\{userId}");
        }

        public bool SavePostPicture(Guid postId, byte[] picture)
        {
            if (picture == null || picture.Length == 0)
                return false;

            var fileExtension = picture.GetImageFormat();

            if (!string.IsNullOrEmpty(fileExtension))
            {
                var postPics = Environment.GetEnvironmentVariable("POST_PIC_DIR") ?? @"%USERPROFILE%\.xws-post-pics";
                postPics = Environment.ExpandEnvironmentVariables(postPics);
                File.WriteAllBytes($"{postPics}\\{postId}", picture);
                _logger.Log(LogLevel.Information, $"Post picture added for post {postId}");
                return true;
            }

            _logger.Log(LogLevel.Warning, $"Post with id {postId} sent an invalid image format");
            return false;
        }

        public void DeletePostPicture(Guid postId)
        {
            var postPics = Environment.GetEnvironmentVariable("POST_PIC_DIR") ?? @"%USERPROFILE%\.xws-post-pics";
            postPics = Environment.ExpandEnvironmentVariables(postPics);
            if (File.Exists($"{postPics}\\{postId}")) File.Delete($"{postPics}\\{postId}");
        }

        public void ChangePostPictureName(Guid oldPostId, Guid newPostId)
        {
            var postPics = Environment.GetEnvironmentVariable("POST_PIC_DIR") ?? @"%USERPROFILE%\.xws-post-pics";
            postPics = Environment.ExpandEnvironmentVariables(postPics);
            if (File.Exists($"{postPics}\\{oldPostId}"))
                File.Move($"{postPics}\\{oldPostId}", $"{postPics}\\{newPostId}");
        }
    }
}