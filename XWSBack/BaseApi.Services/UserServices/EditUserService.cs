﻿using BaseApi.Model.Mongo;
using BaseApi.Services.BaseServices;
using BaseApi.Services.Exceptions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services.UserServices
{
    public class EditUserService : BaseService
    {
        public EditUserService(IMongoClient client) : base(client) {}

        public async Task EditBasicInformations(Guid userId, User newUser)
        {
            var currentUser = await GetUser(userId);

            var editedUser = EditUser(currentUser, newUser);

            await _usersCollection.ReplaceOneAsync(x => x.Id == editedUser.Id, editedUser);
        }

        
        private static User EditUser(User editedUser,User newUser)
        {
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
