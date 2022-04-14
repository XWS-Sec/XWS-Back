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
    public class InterestController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SkillManagementService _skillManagementService;

        public InterestController(SkillManagementService skillManagementService, UserManager<User> userManager)
        {
            _skillManagementService = skillManagementService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var interests = await _skillManagementService.GetSkills(Guid.Parse(userId), "hasInterest");
                
                return Ok(interests.Select(x => x.Name));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(AdjustSkillsDto interestDto)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                await _skillManagementService.Handle(Guid.Parse(userId), interestDto.NewSkills,
                    interestDto.SkillsToRemove, "hasInterest");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}