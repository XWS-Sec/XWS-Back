using AutoMapper;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
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
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly EditUserService _editUserService;

        public UserController(UserManager<User> userManager, IMapper mapper, EditUserService editUserService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _editUserService = editUserService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBasicInformation([FromBody] UpdateUserDto editBasicUserDto)
        { 
            var user = await _userManager.GetUserAsync(User);

            var newUser = _mapper.Map<User>(editBasicUserDto);
            User editedUser;
            try
            {
                editedUser = await _editUserService.EditBasicInformations(user.Id, newUser).ConfigureAwait(false);
            }
            catch(ValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            

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

    }
}
