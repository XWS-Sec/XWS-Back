using BaseApi.Model.Mongo;
using BaseApi.Services.BaseServices;
using BaseApi.Services.Exceptions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services.UserServices
{
    public class EditUserService : BaseUserService<EditUserService>
    {
        public EditUserService(IMongoClient client, ILogger<EditUserService> logger) : base(client,logger) {}

        public async Task<User> EditBasicInformations(Guid userId, User newUser)
        {
            await CheckUniquenessAsync(newUser);
            var currentUser = await GetUser(userId);

            var editedUser = EditUser(currentUser, newUser);

            await _usersCollection.ReplaceOneAsync(x => x.Id == editedUser.Id, editedUser);
            return editedUser;
        }

        private async Task CheckUniquenessAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.UserName))
            {
                bool alreadyExists = await _usersCollection.Find(x => x.UserName.Equals(user.UserName)).AnyAsync();
                if (alreadyExists)
                    throw new ValidationException("Username already exists!");
            }
            if(!string.IsNullOrEmpty(user.Email))
            {
                bool alreadyExists = await _usersCollection.Find(x => x.Email.Equals(user.Email)).AnyAsync();
                if (alreadyExists)
                    throw new ValidationException("Email already exists!");
            }
                
        }

        private static User EditUser(User editedUser,User newUser)
        {
            editedUser.UserName = string.IsNullOrEmpty(newUser.UserName) ? editedUser.UserName : newUser.UserName;
            editedUser.Name = string.IsNullOrEmpty(newUser.Name) ? editedUser.Name : newUser.Name;
            editedUser.Surname = string.IsNullOrEmpty(newUser.Surname) ? editedUser.Surname : newUser.Surname;
            editedUser.IsPrivate = newUser.IsPrivate;
            editedUser.Email = string.IsNullOrEmpty(newUser.Email) ? editedUser.Email : newUser.Email;
            editedUser.PhoneNumber = string.IsNullOrEmpty(newUser.PhoneNumber) ? editedUser.PhoneNumber : newUser.PhoneNumber;
            editedUser.Biography = string.IsNullOrEmpty(newUser.Biography) ? editedUser.Biography : newUser.Biography;

            return editedUser;
        }
    }
}
