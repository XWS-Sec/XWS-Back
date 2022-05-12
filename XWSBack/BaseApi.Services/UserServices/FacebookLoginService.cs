using System;
using System.Net.Http;
using System.Threading.Tasks;
using BaseApi.Services.JsonConvertedContracts;
using BaseApi.Services.PasswordlessSettings;
using Newtonsoft.Json;

namespace BaseApi.Services.UserServices
{
    public class FacebookLoginService
    {
        private readonly FacebookAuthSettings _facebookAuthSettings;
        private const string TokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string UserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,email,picture&access_token={0}";
        private readonly IHttpClientFactory _httpClientFactory;
        
        public FacebookLoginService(FacebookAuthSettings facebookAuthSettings, IHttpClientFactory httpClientFactory)
        {
            _facebookAuthSettings = facebookAuthSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<FacebookValidationResponse> ValidateAccessTokenAsync(string accessToken)
        {
            var formattedUrl = string.Format(TokenValidationUrl, accessToken, _facebookAuthSettings.AppId,
                _facebookAuthSettings.AppSecret);

            var result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);

            if (!result.IsSuccessStatusCode)
                throw new Exception("Couldn't reach facebook api");

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookValidationResponse>(responseAsString);
        }

        public async Task<FacebookUserInfoResponse> GetUserInfoAsync(string accessToken)
        {
            var formattedUrl = string.Format(UserInfoUrl, accessToken);

            var result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);

            if (!result.IsSuccessStatusCode)
                throw new Exception("Couldn't reach facebook api");

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookUserInfoResponse>(responseAsString);
        }
    }
}