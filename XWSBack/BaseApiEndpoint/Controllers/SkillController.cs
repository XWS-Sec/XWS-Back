using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Skills;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.SkillsServices;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class SkillController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SkillManagementService _skillManagementService;

        public SkillController(UserManager<User> userManager, SkillManagementService skillManagementService)
        {
            _userManager = userManager;
            _skillManagementService = skillManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = _userManager.GetUserId(User);
            var skills = await _skillManagementService.GetSkills(Guid.Parse(userId));

            return Ok(skills.Select(x => x.Name));
        }

        [HttpPost]
        public async Task<IActionResult> Post(AdjustSkillsDto skillsDto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _skillManagementService.Handle(Guid.Parse(userId), skillsDto.NewSkills, skillsDto.SkillsToRemove);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok();
        }
    }
}