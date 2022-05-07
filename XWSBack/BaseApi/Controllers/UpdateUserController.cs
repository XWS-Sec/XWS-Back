using AutoMapper;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Model.Mongo;
using BaseApi.Services.PictureServices;
using BaseApi.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class UpdateUserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly EditUserService _editUserService;
        private readonly SignInManager<User> _signInManager;
        private readonly PictureService _pictureService;

        public UpdateUserController(UserManager<User> userManager, IMapper mapper, EditUserService editUserService, SignInManager<User> signInManager, PictureService pictureService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _editUserService = editUserService;
            _signInManager = signInManager;
            _pictureService = pictureService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBasicInformation([FromBody] UpdateUserDto editBasicUserDto)
        { 
            var user = await _userManager.GetUserAsync(User);
            var result = await _signInManager.PasswordSignInAsync(user.UserName, editBasicUserDto.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("Wrong username and password combination!");

            var newUser = _mapper.Map<User>(editBasicUserDto);
            var editedUser = await _editUserService.EditBasicInformations(user.Id, newUser).ConfigureAwait(false);

            return Ok(editedUser);
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordDto changeUserPasswordDto)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.ChangePasswordAsync(user, changeUserPasswordDto.CurrentPassword, changeUserPasswordDto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password succesfully changed!");
        }

        [HttpPut("profileImg")]
        public async Task<IActionResult> ChangePhoto(IFormFile picture)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            _pictureService.DeletePostPicture(userId);
            if (picture != null && picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await picture.CopyToAsync(ms);

                _pictureService.SaveUserPicture(userId, ms.ToArray());
            }

            return Ok("Image changed successfully");
        }

    }
}
