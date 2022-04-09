using System;
using Microsoft.AspNetCore.Mvc;
using PictureApiEndpoint.Extensions;

namespace PictureApiEndpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostPictureController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var path = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
            var expanded = Environment.ExpandEnvironmentVariables(path);

            var pic = $"{expanded}\\{id}";
            var bytes = System.IO.File.ReadAllBytes(pic);
            if (bytes == null || bytes.Length == 0)
                return NotFound();

            return File(bytes, bytes.GetImageFormat());
        }
    }
}