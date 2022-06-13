using System.Threading.Tasks;
using AutoMapper;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class NotificationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public NotificationController(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            return Ok(_mapper.Map<NotificationConfigurationDto>(user.NotificationConfiguration));
        }

        [HttpPost]
        public async Task<IActionResult> Post(NotificationConfigurationDto newConfigs)
        {
            var user = await _userManager.GetUserAsync(User);

            user.NotificationConfiguration = _mapper.Map<NotificationConfiguration>(newConfigs);

            await _userManager.UpdateAsync(user);

            return Ok(newConfigs);
        }
    }
}