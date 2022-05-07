using AutoMapper;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Milestone;
using BaseApi.Model.Mongo;
using BaseApi.Services.MilestoneService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class MilestoneController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MilestoneService _milestoneService;
        private readonly UserManager<User> _userManager;

        public MilestoneController(IMapper mapper, MilestoneService milestoneService, UserManager<User> userManager)
        {
            _mapper = mapper;
            _milestoneService = milestoneService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneDto addMilestoneDto)
        {
            var milestone = _mapper.Map<Milestone>(addMilestoneDto);
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _milestoneService.AddMilestone(userId, milestone).ConfigureAwait(false);

            return Ok(milestone);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveMilestone([FromBody] DeleteMilestoneDto removeMilestoneDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _milestoneService.RemoveMilestone(userId, removeMilestoneDto.Title, removeMilestoneDto.StartTime).ConfigureAwait(false);

            return Ok("Succesfully removed!");
        }
    }
}
