using System;
using System.Threading.Tasks;
using BaseApi.Dto.Users;
using BaseApi.Services.UserServices;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecoveryController : ControllerBase
    {
        private readonly RecoveryService _recoveryService;

        public RecoveryController(RecoveryService recoveryService)
        {
            _recoveryService = recoveryService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string email)
        {
            try
            {
                await _recoveryService.Recover(email);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(RecoverPasswordDto passwordDto)
        {
            try
            {
                await _recoveryService.ResetPassword(passwordDto.Email, passwordDto.Token, passwordDto.NewPassword);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}