using System;

namespace BaseApi.Services.PasswordlessSettings
{
    public class FacebookAuthSettings
    {
        public FacebookAuthSettings()
        {
            var appId = Environment.GetEnvironmentVariable("XWS_FACE_APP_ID");
            if (string.IsNullOrEmpty(appId))
                throw new Exception("XWS_FACE_APP_ID cannot be null or empty");
            AppId = appId;
            
            var appSecret = Environment.GetEnvironmentVariable("XWS_FACE_APP_SECRET");
            if (string.IsNullOrEmpty(appSecret))
                throw new Exception("XWS_FACE_APP_SECRET cannot be null or empty");
            AppSecret = appSecret;
        }
        public string AppId { get; }
        public string AppSecret { get; }
    }
}