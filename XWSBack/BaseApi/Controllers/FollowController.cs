using System;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class FollowController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly UserManager<User> _userManager;

        public FollowController(IMessageSession messageSession, UserManager<User> userManager)
        {
            _messageSession = messageSession;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _messageSession.SendLocal(new BeginGetFollowStatsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                UserId = userId
            }).ConfigureAwait(false);
            
            return Ok();
        }

        [HttpPost("{receiverId}")]
        public async Task<IActionResult> Post([FromRoute] Guid receiverId)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _messageSession.SendLocal(new BeginFollowLinkRequest()
            {
                Sender = userId,
                Receiver = receiverId,
                CorrelationId = Guid.NewGuid()
            }).ConfigureAwait(false);

            return Ok();
        }

        [HttpPut("{followRequestFromId}/{isAccepted}")]
        public async Task<IActionResult> Put([FromRoute] Guid followRequestFromId, [FromRoute] bool isAccepted)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _messageSession.SendLocal(new BeginAnswerFollowRequest()
            {
                CorrelationId = Guid.NewGuid(),
                FollowerId = followRequestFromId,
                ObservedId = userId,
                IsAccepted = isAccepted
            }).ConfigureAwait(false);
            
            return Ok();
        }

        [HttpDelete("{userToUnfollowId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid userToUnfollowId)
        {
            var userId =  Guid.Parse(_userManager.GetUserId(User));
            await _messageSession.SendLocal(new BeginUnfollowRequest()
            {
                Receiver = userToUnfollowId,
                Sender = userId,
                CorrelationId = Guid.NewGuid()
            }).ConfigureAwait(false);

            return Ok();
        }
    }
}