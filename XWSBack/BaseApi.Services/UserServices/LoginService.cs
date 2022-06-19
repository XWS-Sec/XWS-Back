using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using BaseApi.Services.Extensions;
using BaseApi.Services.MailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NServiceBus;
using Users.Graph.Messages;

namespace BaseApi.Services.UserServices
{
    public class LoginService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly FacebookLoginService _facebookLoginService;
        private readonly IMessageSession _session;
        private readonly EmailService _emailService;
        private const string TokenProviderName = "NPTokenProvider";
        private const string TokenReason = "passwordless login";

        public LoginService(SignInManager<User> signInManager, UserManager<User> userManager, FacebookLoginService facebookLoginService, IMessageSession session, EmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _facebookLoginService = facebookLoginService;
            _session = session;
            _emailService = emailService;
        }

        public async Task SendToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            ValidateUser(user);

            var token = await _userManager.GenerateUserTokenAsync(user, TokenProviderName, TokenReason);

            token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            
            await _emailService.SendPasswordlessToken(email, token, user.Id);
        }

        public async Task LoginCommon(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            ValidateUser(user);
            
            if (!await _userManager.CheckPasswordAsync(user, password))
                throw new ValidationException("Passwords mismatch");

            await Login(user);
        }

        public async Task LoginPasswordless(string accessToken, string issuer)
        {
            if (issuer == "Facebook")
            {
                await HandleFacebook(accessToken);
            }
            else
            {
                throw new ValidationException("Unknown issuer " + issuer);
            }
        }

        public async Task LoginOurPasswordless(string accessToken, Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            ValidateUser(user);

            accessToken = Encoding.UTF8.GetString(Convert.FromBase64String(accessToken));
            
            var isValid = await _userManager.VerifyUserTokenAsync(user, TokenProviderName, TokenReason, accessToken);
            if (!isValid)
            {
                throw new ValidationException("Token is not valid!");
            }

            await Login(user);
        }
        
        private async Task HandleFacebook(string accessToken)
        {
            var validationResponse = await _facebookLoginService.ValidateAccessTokenAsync(accessToken);

            if (!validationResponse.Data.IsValid)
                throw new ValidationException("Access token deemed invalid by Facebook");

            var userData = await _facebookLoginService.GetUserInfoAsync(accessToken);

            var user = await _userManager.FindByEmailAsync(userData.Email);
            if (user == null)
            {
                user = new User()
                {
                    UserName = userData.Email.Split('@')[0],
                    Email = userData.Email,
                    EmailConfirmed = true,
                    Name = userData.FirstName,
                    Surname = userData.LastName,
                };
                await _userManager.CreateAsync(user);
                
                await _session.Send(new CreateNodeRequest()
                {
                    NewUserId = user.Id
                }).ConfigureAwait(false);
            }

            await Login(user);
        }
        
        private async Task Login(User user)
        {
            await _signInManager.SignInAsync(user, false, null);
        }

        private static void ValidateUser(User user)
        {
            if (user == null)
                throw new ValidationException("User not found");

            if (!user.EmailConfirmed)
                throw new ValidationException("Email not verified");
        }
    }
}