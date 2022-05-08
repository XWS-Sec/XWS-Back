using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services.BaseServices
{
    public abstract class BaseUserService<T>
    {
        protected readonly IMongoCollection<User> _usersCollection;
        protected readonly ILogger<T> _logger;

        public BaseUserService(IMongoClient client, ILogger<T> logger)
        {
            _usersCollection = client.GetDatabase("Users").GetCollection<User>("users");
            _logger = logger;
        }

        public async Task<User> GetUser(Guid userId)
        {
            var user = await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User with id:{userId} doesn't exist!",userId);
                throw new ValidationException("User doesn't exist!");
            }

            return user;
        }
    }
}
