using System;
using BaseApi.Services.Exceptions;

namespace BaseApi.Services.PasswordlessSettings
{
    public class FacebookAuthSettings
    {
        public FacebookAuthSettings()
        {
            var appId = Environment.GetEnvironmentVariable("XWS_FACE_APP_ID");
            if (string.IsNullOrEmpty(appId))
                throw new ValidationException("XWS_FACE_APP_ID cannot be null or empty");

            if (appId.StartsWith("{"))
                appId = appId.Substring(1, appId.Length - 2);
            
            AppId = appId;
            
            var appSecret = Environment.GetEnvironmentVariable("XWS_FACE_APP_SECRET");
            if (string.IsNullOrEmpty(appSecret))
                throw new ValidationException("XWS_FACE_APP_SECRET cannot be null or empty");

            if (appSecret.StartsWith("{"))
                appSecret = appSecret.Substring(1, appSecret.Length - 2);
            
            AppSecret = appSecret;
        }
        public string AppId { get; }
        public string AppSecret { get; }
    }
}