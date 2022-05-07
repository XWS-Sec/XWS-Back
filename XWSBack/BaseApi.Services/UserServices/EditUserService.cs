﻿using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services.UserServices
{
    public class EditUserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public EditUserService(IMongoClient client)
        {
            _usersCollection = client.GetDatabase("Users").GetCollection<User>("Users");
        }

        public async Task EditBasicInformations(Guid userId, User newUser)
        {
            var currentUser = await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            if (currentUser == null)
                throw new BadRequestException("User doesn't exist!");

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
