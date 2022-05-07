using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services.BaseServices
{
    public class BaseService
    {
        protected readonly IMongoCollection<User> _usersCollection;

        public BaseService(IMongoClient client)
        {
            _usersCollection = client.GetDatabase("Users").GetCollection<User>("Users"); ;
        }

        public async Task<User> GetUser(Guid userId)
        {
            var user = await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new BadRequestException("User doesn't exist!");

            return user;
        }
    }
}
