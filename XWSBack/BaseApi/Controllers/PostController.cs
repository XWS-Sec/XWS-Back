using System;
using System.IO;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Posts;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : SyncController
    {
        private readonly IMessageSession _session;
        private readonly UserManager<User> _userManager;

        public PostController(UserManager<User> userManager, IMessageSession session, IMemoryCache cache) : base(cache)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> Get(int page, Guid specificUser)
        {
            var userId = _userManager.GetUserId(User);
            var userGuidId = string.IsNullOrEmpty(userId) 
                ? Guid.Empty
                : Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginGetPostsRequest()
            {
                Page = page,
                CorrelationId = Guid.NewGuid(),
                UserId = userGuidId,
                RequestedUserId = specificUser
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
        
        [HttpPost]
        [TypeFilter(typeof(CustomAuthorizeAttribute))]
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

            var request = new BeginNewPostRequest()
            {
                Picture = bytes,
                Text = newPost.Text,
                CorrelationId = Guid.NewGuid(),
                UserId = userId
            };
            
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            return ReturnBaseNotification(response);
        }

        [HttpPut("{postId}")]
        [TypeFilter(typeof(CustomAuthorizeAttribute))]
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

            var request = new BeginEditPostRequest()
            {
                Picture = bytes,
                Text = postDto.Text,
                CorrelationId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                RemoveOldPic = postDto.RemovedPicture
            };
            
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
    }
}