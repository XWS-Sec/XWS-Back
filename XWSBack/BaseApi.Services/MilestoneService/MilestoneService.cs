using BaseApi.Model.Mongo;
using BaseApi.Services.Exceptions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services.MilestoneService
{
    public class MilestoneService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public MilestoneService(IMongoCollection<User> usersCollection)
        {
            _usersCollection = usersCollection;
        }

        public async Task AddMilestone(Guid userId, Milestone milestone)
        {
            var user = await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new BadRequestException("User doesn't exist!");


            if (user.Experiences == null)
                user.Experiences = new List<Milestone>();

            user.Experiences.Add(milestone);
            await _usersCollection.ReplaceOneAsync(x => x.Id == userId, user);
        }

        public async Task RemoveMilestone(Guid userId, string title, DateTime startTime)
        {
            var user = await _usersCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new BadRequestException("User doesn't exist!");

            if (user.Experiences == null)
                user.Experiences = new List<Milestone>();

            user.Experiences = user.Experiences.Where(x => !(x.Title.Equals(title) && x.StartDateTime.Equals(startTime))).ToList();
            await _usersCollection.ReplaceOneAsync(x => x.Id == userId, user);
        }
    }
}
