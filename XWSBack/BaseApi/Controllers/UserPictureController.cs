using System;
using System.Runtime.CompilerServices;
using BaseApi.Services.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPictureController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var path = Environment.GetEnvironmentVariable("POST_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
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
    }
}