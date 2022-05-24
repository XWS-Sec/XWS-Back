﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Users;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
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
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var users = await _userCollection.FindAsync(x => x.Username.Contains(criteria) ||
                                                             x.Name.Contains(criteria) ||
                                                             x.Surname.Contains(criteria));

            return Ok(users.ToEnumerable().Where(x => x.Id != userId));
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var users = await _userCollection.FindAsync(x => x.Username == username);

            return Ok(users.FirstOrDefault());
        }
    }
}