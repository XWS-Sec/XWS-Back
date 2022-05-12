using System;
using Newtonsoft.Json;

namespace BaseApi.Services.JsonConvertedContracts
{
    public class FacebookUserInfoResponse
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("picture")]
        public PictureObject Picture { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class PictureObject
    {
        [JsonProperty("data")]
        public PictureData Data { get; set; }
    }

    public class PictureData
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }
    }
}