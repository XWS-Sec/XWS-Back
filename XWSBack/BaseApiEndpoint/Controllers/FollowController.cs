using System;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Following;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.FollowServices;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class FollowController : ControllerBase
    {
        private readonly CreateFollowLinkService _createFollowLinkService;
        private readonly UserManager<User> _userManager;
        private readonly GetFollowStatsService _getFollowStatsService;
        private readonly FollowRequestService _followRequestService;
        private readonly UnfollowService _unfollowService;

        public FollowController(CreateFollowLinkService createFollowLinkService, UserManager<User> userManager, GetFollowStatsService getFollowStatsService, FollowRequestService followRequestService, UnfollowService unfollowService)
        {
            _createFollowLinkService = createFollowLinkService;
            _userManager = userManager;
            _getFollowStatsService = getFollowStatsService;
            _followRequestService = followRequestService;
            _unfollowService = unfollowService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var response = new FollowInfoDto()
            {
                Followers = await _getFollowStatsService.GetFollowers(userId),
                FollowerRequests = await _getFollowStatsService.GetFollowRequests(userId),
                Following = await _getFollowStatsService.GetFollowing(userId)
            };
            return Ok(response);
        }

        [HttpPost("{receiverId}")]
        public async Task<IActionResult> Post([FromRoute] Guid receiverId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _createFollowLinkService.Create(Guid.Parse(userId), receiverId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPut("{followRequestFromId}/{isAccepted}")]
        public async Task<IActionResult> Put([FromRoute] Guid followRequestFromId, [FromRoute] bool isAccepted)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (isAccepted)
                    await _followRequestService.Accept(Guid.Parse(userId), followRequestFromId);
                else
                    await _followRequestService.Decline(Guid.Parse(userId), followRequestFromId);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpDelete("{userToUnfollowId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid userToUnfollowId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _unfollowService.Unfollow(Guid.Parse(userId), userToUnfollowId);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}