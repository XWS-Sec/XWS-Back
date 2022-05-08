using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using BaseApi.Dto;
using BaseApi.Model.Mongo;
using BaseApi.Services.PictureServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Users.Graph.Messages;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMessageSession _messageSession;
        private readonly PictureService _pictureService;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RegisterController(UserManager<User> userManager, IMapper mapper, IMessageSession messageSession,
            PictureService pictureService, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _messageSession = messageSession;
            _pictureService = pictureService;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto newUser)
        {
            var mappedUser = _mapper.Map<User>(newUser);
            var result = await _userManager.CreateAsync(mappedUser, newUser.Password).ConfigureAwait(false);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _roleManager.CreateAsync(new Role("User"));
            await _userManager.AddToRoleAsync(mappedUser, "User");

            await _messageSession.Send(new CreateNodeRequest()
            {
                NewUserId = mappedUser.Id
            }).ConfigureAwait(false);

            if (newUser.Picture != null && newUser.Picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await newUser.Picture.CopyToAsync(ms);

                _pictureService.SaveUserPicture(mappedUser.Id, ms.ToArray());
            }

            return Ok(mappedUser);
        }
    }
}