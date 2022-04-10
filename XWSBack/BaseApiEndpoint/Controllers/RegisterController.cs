using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using BaseApi.Dto;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.PictureServices;
using Services.UserServices;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly CreateUserNodeService _nodeService;
        private readonly PictureService _pictureService;
        private readonly RoleManager<Role> _roleManager;

        public RegisterController(UserManager<User> userManager, IMapper mapper, CreateUserNodeService nodeService, PictureService pictureService, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _nodeService = nodeService;
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
            
            await _nodeService.CreateNode(mappedUser.Id).ConfigureAwait(false);

            if (newUser.Picture != null && newUser.Picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await newUser.Picture.CopyToAsync(ms);
            
                _pictureService.SavePicture(mappedUser.Id, ms.ToArray());
            }
            return Ok(mappedUser);
        }
    }
}