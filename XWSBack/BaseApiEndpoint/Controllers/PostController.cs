using System;
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
        private readonly EditPostService _editPostService;

        public PostController(UserManager<User> userManager, CreatePostService createPostService, EditPostService editPostService)
        {
            _userManager = userManager;
            _createPostService = createPostService;
            _editPostService = editPostService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] PostDto newPost)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));

            byte[] bytes = null;
            if (newPost.Picture != null && newPost.Picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await newPost.Picture.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            
            var postId = await _createPostService.Create(userId, newPost.Text, bytes);
            return Ok(postId);
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> Put([FromRoute] Guid postId, [FromForm] PostDto postDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));

            byte[] bytes = null;
            if (postDto.Picture != null && postDto.Picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await postDto.Picture.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            try
            {
                await _editPostService.Edit(userId, postId, postDto.Text, bytes, postDto.RemovedPicture);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok(postId);
        }
    }
}