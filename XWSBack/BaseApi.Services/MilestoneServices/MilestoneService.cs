using BaseApi.Model.Mongo;
using BaseApi.Services.BaseServices;
using BaseApi.Services.Exceptions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services.MilestoneServices
{
    public class MilestoneService : BaseUserService<MilestoneService>
    {

        public MilestoneService(IMongoClient client,ILogger<MilestoneService> logger)  : base(client,logger)
        {
        }

        public async Task AddMilestone(Guid userId, Milestone milestone)
        {
            var user = await GetUser(userId);

            if (user.Experiences == null)
                user.Experiences = new List<Milestone>();

            if (user.Experiences.Any(x => x.Title.Equals(milestone.Title) && x.StartDateTime.Equals(milestone.StartDateTime)))
                throw new ValidationException("User already has milestone with same title and start time!");

            user.Experiences.Add(milestone);
            await _usersCollection.ReplaceOneAsync(x => x.Id == userId, user);
        }

        public async Task RemoveMilestone(Guid userId, string title, DateTime startTime)
        {
            var user = await GetUser(userId);

            if (user.Experiences == null)
                throw new ValidationException("User doesn't have any milestone!");

            if (!user.Experiences.Any(x => x.Title.Equals(title) && x.StartDateTime.Equals(startTime)))
                throw new ValidationException("User doesn't have such milestone!");

            user.Experiences = user.Experiences.Where(x => !(x.Title.Equals(title) && x.StartDateTime.Equals(startTime))).ToList();
            await _usersCollection.ReplaceOneAsync(x => x.Id == userId, user);
        }
    }
}
