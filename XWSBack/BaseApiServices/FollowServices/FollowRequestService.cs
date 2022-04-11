using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using BaseApiModel.Mongo;
using Microsoft.AspNetCore.Identity;
using Neo4jClient;

namespace Services.FollowServices
{
    public class FollowRequestService
    {
        private readonly UserManager<User> _userManager;
        private readonly IGraphClient _client;

        public FollowRequestService(UserManager<User> userManager, IGraphClient client)
        {
            _userManager = userManager;
            _client = client;
        }

        public async Task Accept(Guid userId, Guid followRequestFromId)
        {
            await Decline(userId, followRequestFromId);

            await CreateFollowLink(userId, followRequestFromId);
        }

        public async Task Decline(Guid userId, Guid followRequestFromId)
        {
            var requests = await GetRequests(userId, followRequestFromId);

            if (!requests.Any())
            {
                throw new Exception($"Nodes {userId} and {followRequestFromId} are not related");
            }

            await DeleteFollowRequestLink(userId, followRequestFromId);
        }
        
        private async Task<IEnumerable<FollowRequest>> GetRequests(Guid userId, Guid followRequestFromId)
        {
            /*
             * m - Me
             * h - Him/Her
             * (h)-[f:followRequest]->(m) him that has a link of followRequest to me
             */
            return await _client.Cypher.Match("(m:UserNode), (h:UserNode)")
                .Where((UserNode m, UserNode h) => m.UserId == userId && h.UserId == followRequestFromId)
                .Match("(h)-[f:followRequest]->(m)")
                .Return(f => f.As<FollowRequest>())
                .ResultsAsync;
        }

        private async Task DeleteFollowRequestLink(Guid userId, Guid followRequestFromId)
        {
            await _client.Cypher.Match("(m:UserNode), (h:UserNode)")
                .Where((UserNode m, UserNode h) => m.UserId == userId && h.UserId == followRequestFromId)
                .Match("(h)-[f:followRequest]->(m)")
                .Delete("f")
                .ExecuteWithoutResultsAsync();
        }

        private async Task CreateFollowLink(Guid userId, Guid followRequestFromId)
        {
            await _client.Cypher.Match("(m:UserNode), (h:UserNode)")
                .Where((UserNode m, UserNode h) => m.UserId == userId && h.UserId == followRequestFromId)
                .Create("(h)-[f:follows $req]->(m)")
                .WithParam("req", new Follow()
                {
                    CreatedDate = DateTime.Now,
                    FollowId = Guid.NewGuid()
                }).ExecuteWithoutResultsAsync();
        }
    }
}