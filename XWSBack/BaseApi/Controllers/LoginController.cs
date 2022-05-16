using System;
using System.Threading.Tasks;
using BaseApi.Dto;
using BaseApi.Model.Mongo;
using BaseApi.Services.UserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly LoginService _loginService;
        private readonly UserManager<User> _userManager;

        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager, LoginService loginService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loginService = loginService;
        }

        [HttpPost("api/login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            try
            {
                await _loginService.LoginCommon(userDto.Username, userDto.Password);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(await _userManager.FindByNameAsync(userDto.Username));
        }

        [HttpPost("api/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("api/login/passwordless")]
        public async Task<IActionResult> Passwordless(string accessToken, string issuer)
        {
            try
            {
                await _loginService.LoginPasswordless(accessToken, issuer);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpGet("api/login/token")]
        public async Task<IActionResult> GetToken(string email)
        {
            try
            {
                await _loginService.SendToken(email);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpGet("api/login/our/passwordless/{accessToken}/{id}")]
        public async Task<IActionResult> OurPasswordless(string accessToken, Guid id)
        {
            try
            {
                await _loginService.LoginOurPasswordless(accessToken, id);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            return Redirect("/swagger");
        }
    }
}