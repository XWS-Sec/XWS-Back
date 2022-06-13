using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Services
{
    public class GetFollowStatsService
    {
        private readonly IGraphClient _client;

        public GetFollowStatsService(IGraphClient client)
        {
            _client = client;
        }
        
        public async Task<IEnumerable<Guid>> GetIdsFromMe(Guid userId, string linkName)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match($"(u)-[f:{linkName}]->(k)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }

        public async Task<IEnumerable<Guid>> GetIdsToMe(Guid userId, string linkName)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match($"(k)-[f:{linkName}]->(u)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }
    }
}