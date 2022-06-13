using System;
using System.Threading.Tasks;
using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BaseApi.Services.UserServices
{
    public class ChangeUserVisibilityService
    {
        private readonly UserManager<User> _userManager;

        public ChangeUserVisibilityService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task ChangeVisibility(Guid userId, bool newVisibility)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new ValidationException("User with that id is not found");

            if (user.IsPrivate == newVisibility)
                throw new ValidationException("User is already of that visibility");

            user.IsPrivate = newVisibility;
            await _userManager.UpdateAsync(user);
        }
    }
}