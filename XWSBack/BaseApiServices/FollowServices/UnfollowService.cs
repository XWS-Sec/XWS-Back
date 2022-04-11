using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using Neo4jClient;

namespace Services.FollowServices
{
    public class UnfollowService
    {
        private readonly IGraphClient _client;

        public UnfollowService(IGraphClient client)
        {
            _client = client;
        }

        public async Task Unfollow(Guid userId, Guid userToUnfollowId)
        {
            var followLink = await GetLink(userId, userToUnfollowId);

            if (!followLink.Any())
            {
                throw new Exception($"User {userId} does not follow user {userToUnfollowId}");
            }

            await DeleteLink(userId, userToUnfollowId);
        }

        private async Task<IEnumerable<Follow>> GetLink(Guid userId, Guid userToUnfollowId)
        {
            return await _client.Cypher.Match("(m:UserNode), (h:UserNode)")
                .Where((UserNode m, UserNode h) => m.UserId == userId && h.UserId == userToUnfollowId)
                .Match("(m)-[f:follows]->(h)")
                .Return(f => f.As<Follow>())
                .ResultsAsync;
        }

        private async Task DeleteLink(Guid userId, Guid userToUnfollowId)
        {
            await _client.Cypher.Match("(m:UserNode), (h:UserNode)")
                .Where((UserNode m, UserNode h) => m.UserId == userId && h.UserId == userToUnfollowId)
                .Match("(m)-[f:follows]->(h)")
                .Delete("f")
                .ExecuteWithoutResultsAsync();
        }
    }
}