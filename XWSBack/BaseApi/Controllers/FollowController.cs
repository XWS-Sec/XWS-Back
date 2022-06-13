using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.CustomAttributes;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;
using Shared.Custom;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class FollowController : SyncController
    {
        private readonly IMessageSession _messageSession;
        private readonly UserManager<User> _userManager;

        public FollowController(IMessageSession messageSession, UserManager<User> userManager, IMemoryCache memoryCache) : base(memoryCache)
        {
            _messageSession = messageSession;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginGetFollowStatsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                UserId = userId
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);
            
            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }

        [HttpPost("{receiverId}")]
        public async Task<IActionResult> Post([FromRoute] Guid receiverId)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginFollowLinkRequest()
            {
                Sender = userId,
                Receiver = receiverId,
                CorrelationId = Guid.NewGuid()
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }

        [HttpPut("{followRequestFromId}/{isAccepted}")]
        public async Task<IActionResult> Put([FromRoute] Guid followRequestFromId, [FromRoute] bool isAccepted)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginAnswerFollowRequest()
            {
                CorrelationId = Guid.NewGuid(),
                FollowerId = followRequestFromId,
                ObservedId = userId,
                IsAccepted = isAccepted
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            
            return ReturnBaseNotification(response);
        }

        [HttpDelete("{userToUnfollowId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid userToUnfollowId)
        {
            var userId =  Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginUnfollowRequest()
            {
                Receiver = userToUnfollowId,
                Sender = userId,
                CorrelationId = Guid.NewGuid()
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }

        [HttpPost("block/{userToBlock}")]
        public async Task<IActionResult> Block(Guid userToBlock)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginBlockUnblockRequest()
            {
                CorrelationId = Guid.NewGuid(),
                IsBlock = true,
                UserId = userId,
                OtherUserId = userToBlock
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
        
        [HttpPost("unblock/{userToUnblock}")]
        public async Task<IActionResult> Unblock(Guid userToUnblock)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginBlockUnblockRequest()
            {
                CorrelationId = Guid.NewGuid(),
                IsBlock = false,
                UserId = userId,
                OtherUserId = userToUnblock
            };
            await _messageSession.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
    }
}