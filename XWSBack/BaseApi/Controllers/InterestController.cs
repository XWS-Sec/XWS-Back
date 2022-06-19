using System;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Skills;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class InterestController : SyncController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageSession _session;

        public InterestController(UserManager<User> userManager, IMessageSession session, IMemoryCache cache) : base(cache)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginGetSkillsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                LinkName = "hasInterest",
                UserId = userId
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AdjustSkillsDto interestDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginAdjustSkillsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                NewSkills = interestDto.NewSkills,
                SkillsToRemove = interestDto.SkillsToRemove,
                UserId = userId,
                LinkName = "hasInterest"
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
    }
}