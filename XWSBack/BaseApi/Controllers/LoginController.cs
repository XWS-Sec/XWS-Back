﻿using System.Threading.Tasks;
using BaseApi.Dto;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("api/login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            var result = await _signInManager.PasswordSignInAsync(userDto.Username, userDto.Password, false, false);
            return result.Succeeded
                ? Ok(await _userManager.FindByNameAsync(userDto.Username))
                : BadRequest("Incorrect password");
        }

        [HttpPost("api/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}