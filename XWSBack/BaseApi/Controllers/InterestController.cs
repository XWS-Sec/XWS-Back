using System;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Skills;
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
    public class InterestController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageSession _session;

        public InterestController(UserManager<User> userManager, IMessageSession session)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _session.SendLocal(new BeginGetSkillsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                LinkName = "hasInterest",
                UserId = userId
            }).ConfigureAwait(false);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post(AdjustSkillsDto interestDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _session.SendLocal(new BeginAdjustSkillsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                NewSkills = interestDto.NewSkills,
                SkillsToRemove = interestDto.SkillsToRemove,
                UserId = userId,
                LinkName = "hasInterest"
            }).ConfigureAwait(false);

            return Ok();
        }
    }
}