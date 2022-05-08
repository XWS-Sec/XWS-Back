using System;
using BaseApi.Services.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostPictureController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var path = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-post-pics";
            var expanded = Environment.ExpandEnvironmentVariables(path);

            var pic = $"{expanded}\\{id}";
            try
            {
                var bytes = System.IO.File.ReadAllBytes(pic);
                return File(bytes, bytes.GetImageFormat());
            }
            catch (Exception e)
            {
                return NotFound();
            }

        }
    }
}