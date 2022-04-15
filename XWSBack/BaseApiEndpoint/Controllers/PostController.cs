using System.IO;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Posts;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.PostServices;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class PostController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly CreatePostService _createPostService;

        public PostController(UserManager<User> userManager, CreatePostService createPostService)
        {
            _userManager = userManager;
            _createPostService = createPostService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] PostDto newPost)
        {
            var user = await _userManager.GetUserAsync(User);

            byte[] bytes = null;
            if (newPost.Picture != null && newPost.Picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await newPost.Picture.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            
            var postId = await _createPostService.Create(user, newPost.Text, bytes);
            return Ok(postId);
        }
    }
}