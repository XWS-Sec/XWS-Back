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
using BaseApi.Controllers.Base;
using BaseApi.Messages;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;

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
        private readonly ChangeUserVisibilityService _changeUserVisibilityService;
        private readonly IMessageSession _session;

        public UserController(UserManager<User> userManager, IMapper mapper, EditUserService editUserService, ChangeUserVisibilityService changeUserVisibilityService, IMessageSession session)
        {
            _userManager = userManager;
            _mapper = mapper;
            _editUserService = editUserService;
            _changeUserVisibilityService = changeUserVisibilityService;
            _session = session;
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

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if(user!= null)
                return Ok(user);


            return BadRequest("User not found!");
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

        [HttpPut("/changeVisibility/{isPrivate}")]
        public async Task<IActionResult> ChangeVisibility(bool isPrivate)
        {
            var user = Guid.Parse(_userManager.GetUserId(User));
            try
            {
                await _changeUserVisibilityService.ChangeVisibility(user, isPrivate);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            if (!isPrivate)
            {
                var request = new BeginAnswerAllFollowRequestsRequest()
                {
                    CorrelationId = Guid.NewGuid(),
                    UserId = user
                };

                await _session.SendLocal(request).ConfigureAwait(false);   
            }
            
            return Ok("Successfully changed the visibility");
        }
    }
}
