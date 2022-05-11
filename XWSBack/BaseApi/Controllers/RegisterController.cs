using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using BaseApi.Dto;
using BaseApi.Model.Mongo;
using BaseApi.Services.MailService;
using BaseApi.Services.PictureServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly EmailService _emailService;

        public RegisterController(UserManager<User> userManager, IMapper mapper, IMessageSession messageSession,
            PictureService pictureService, RoleManager<Role> roleManager, EmailService emailService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _messageSession = messageSession;
            _pictureService = pictureService;
            _roleManager = roleManager;
            _emailService = emailService;
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

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(mappedUser);
            code = HttpUtility.UrlEncode(code);
            try
            {
                await _emailService.VerifyEmail(mappedUser.Email, mappedUser.Id, code);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok(mappedUser);
        }

        [HttpGet("Confirm")]
        public async Task<IActionResult> Confirm(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Redirect("/swagger");
        }
    }
}