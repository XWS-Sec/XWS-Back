using System.Linq;
using System.Threading.Tasks;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using BaseApi.Services.MailService;
using Microsoft.AspNetCore.Identity;

namespace BaseApi.Services.UserServices
{
    public class RecoveryService
    {
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;

        public RecoveryService(EmailService emailService, UserManager<User> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task Recover(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new ValidationException("User with that address does not exist");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.RecoverAccount(email, token);
        }

        public async Task ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new ValidationException("User with that address does not exist");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new ValidationException(string.Join("\n", result.Errors.Select(x => x.Description)));
        }
    }
}