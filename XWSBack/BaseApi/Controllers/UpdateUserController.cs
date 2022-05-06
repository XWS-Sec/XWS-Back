using AutoMapper;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using BaseApi.Services.PictureServices;
using BaseApi.Services.UserServices;
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
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly EditUserService _editUserService;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, IMapper mapper, EditUserService editUserService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _editUserService = editUserService;
            _signInManager = signInManager;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBasicInformation([FromBody] EditBasicUserDto editBasicUserDto)
        { 
            var user = await _userManager.GetUserAsync(User);
            var result = await _signInManager.PasswordSignInAsync(user.UserName, editBasicUserDto.Password, false, false);
            if (!result.Succeeded)
                Unauthorized("Wrong username and password combination!");

            var newUser = _mapper.Map<User>(editBasicUserDto);
            await _editUserService.EditBasicInformations(user.Id, newUser).ConfigureAwait(false);

            return Ok(newUser);
        }





    }
}
