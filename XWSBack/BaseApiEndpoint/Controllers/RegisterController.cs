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

        public RegisterController(UserManager<User> userManager, IMapper mapper, CreateUserNodeService nodeService, PictureService pictureService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _nodeService = nodeService;
            _pictureService = pictureService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto newUser)
        {
            var mappedUser = _mapper.Map<User>(newUser);
            var result = await _userManager.CreateAsync(mappedUser, newUser.Password).ConfigureAwait(false);
            
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _nodeService.CreateNode(mappedUser.Id).ConfigureAwait(false);

            using var ms = new MemoryStream();
            await newUser.Picture.CopyToAsync(ms);
            
            _pictureService.SavePicture(mappedUser.Id, ms.ToArray());
            return Ok(mappedUser);
        }
    }
}