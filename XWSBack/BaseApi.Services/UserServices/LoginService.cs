using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BaseApi.Services.UserServices
{
    public class LoginService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public LoginService(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task LoginCommon(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new ValidationException("User not found");

            if (!user.EmailConfirmed)
                throw new ValidationException("Email not verified");
            
            if (!await _userManager.CheckPasswordAsync(user, password))
                throw new ValidationException("Passwords mismatch");

            await Login(user);
        }

        private async Task Login(User user)
        {
            await _signInManager.SignInAsync(user, false, null);
        }
    }
}