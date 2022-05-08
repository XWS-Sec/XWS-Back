using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Model.Mongo;
using BaseApi.Services.Extensions;
using BaseApi.Services.PictureServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPictureController : ControllerBase
    {

        private readonly PictureService _pictureService;
        private readonly UserManager<User> _userManager;

        public UserPictureController(PictureService pictureService, UserManager<User> userManager)
        {
            _pictureService = pictureService;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var path = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
            var expanded = Environment.ExpandEnvironmentVariables(path);

            var pic = $"{expanded}\\{id}";
            byte[] bytes = null;

            try
            {
                if (System.IO.File.Exists(pic)) bytes = System.IO.File.ReadAllBytes(pic);
                return File(bytes, bytes.GetImageFormat());
            }
            catch (Exception e)
            {
                return NotFound();
            }

        }
        //Used for creating and updating
        [HttpPost]
        [TypeFilter(typeof(CustomAuthorizeAttribute))]
        public async Task<IActionResult> SetPhoto(IFormFile picture)
        {

            var userId = Guid.Parse(_userManager.GetUserId(User));

            if (picture != null && picture.Length != 0)
            {
                using var ms = new MemoryStream();
                await picture.CopyToAsync(ms);

                _pictureService.SaveUserPicture(userId, ms.ToArray());
            }

            return Ok("Profile image added successfully");
        }

        [HttpDelete]
        [TypeFilter(typeof(CustomAuthorizeAttribute))]
        public IActionResult DeletePhoto()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));


            _pictureService.DeleteUserPicture(userId);
            return Ok("Profile image removed!");
        }
    }
}