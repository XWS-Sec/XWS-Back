using Microsoft.AspNetCore.Http;

namespace BaseApi.Dto.Posts
{
    public class PostDto
    {
        public IFormFile Picture { get; set; }
        public string Text { get; set; }
        public bool RemovedPicture { get; set; }
    }
}