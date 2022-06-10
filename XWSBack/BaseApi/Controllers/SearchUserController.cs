using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Messages.Dtos;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchUserController : ControllerBase
    {
        private readonly IMongoCollection<SearchedUserDto> _userCollection;
        private readonly UserManager<User> _userManager;

        public SearchUserController(IMongoClient client, UserManager<User> userManager)
        {
            _userManager = userManager;
            _userCollection = client.GetDatabase("Users").GetCollection<SearchedUserDto>("users");
        }

        [HttpGet]
        public async Task<IActionResult> Get(string criteria)
        {
            var userId = Guid.Empty;
            try
            {
                userId = Guid.Parse(_userManager.GetUserId(User));
            }
            catch (Exception e) {}
            var users = await _userCollection.FindAsync(x => x.Username.Contains(criteria) ||
                                                             x.Name.Contains(criteria) ||
                                                             x.Surname.Contains(criteria));

            return Ok(JsonConvert.SerializeObject(users.ToEnumerable().Where(x => x.Id != userId)));
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var users = await _userCollection.FindAsync(x => x.Username == username);

            return Ok(JsonConvert.SerializeObject(users.FirstOrDefault()));
        }

        [HttpGet("id/{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return user == null
                ? BadRequest("User with that id is not present")
                : Ok(JsonConvert.SerializeObject(new SearchedUserDto()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Id = user.Id,
                    Username = user.UserName,
                    IsPrivate = user.IsPrivate
                }));
        }
    }
}