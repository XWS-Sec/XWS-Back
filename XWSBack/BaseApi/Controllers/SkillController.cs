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
using Neo4jClient.Cypher;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class SkillController : SyncController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageSession _session;

        public SkillController(UserManager<User> userManager, IMessageSession session, IMemoryCache cache) : base(cache)
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
                LinkName = "hasSkill",
                UserId = userId
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            
            return ReturnBaseNotification(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AdjustSkillsDto skillsDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginAdjustSkillsRequest()
            {
                CorrelationId = Guid.NewGuid(),
                UserId = userId,
                LinkName = "hasSkill",
                NewSkills = skillsDto.NewSkills,
                SkillsToRemove = skillsDto.SkillsToRemove
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            
            return ReturnBaseNotification(response);
        }
    }
}