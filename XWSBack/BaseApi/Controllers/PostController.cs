using System;
using System.IO;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Posts;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class PostController : ControllerBase
    {
        private readonly IMessageSession _session;
        private readonly UserManager<User> _userManager;

        public PostController(UserManager<User> userManager, IMessageSession session)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> Get(int page, Guid specificUser)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _session.SendLocal(new BeginGetPostsRequest()
            {
                Page = page,
                CorrelationId = Guid.NewGuid(),
                UserId = userId,
                RequestedUserId = specificUser
            }).ConfigureAwait(false);
            
            return Ok();
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

            await _session.SendLocal(new BeginNewPostRequest()
            {
                Picture = bytes,
                Text = newPost.Text,
                CorrelationId = Guid.NewGuid(),
                UserId = userId
            }).ConfigureAwait(false);
            return Ok();
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

            await _session.SendLocal(new BeginEditPostRequest()
            {
                Picture = bytes,
                Text = postDto.Text,
                CorrelationId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                RemoveOldPic = postDto.RemovedPicture
            }).ConfigureAwait(false);
            
            return Ok();
        }
    }
}