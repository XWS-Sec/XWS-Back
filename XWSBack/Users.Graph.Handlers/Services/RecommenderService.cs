using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Services
{
    public class RecommenderService
    {
        private readonly IGraphClient _client;

        public RecommenderService(IGraphClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<Guid>> GetPotentialLinks(Guid userId)
        {
            return await _client.Cypher
                .Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match("(u)-[:follows]->(t)-[:follows]->(z)")
                .Where("not (u)-[:follows]->(z) and not (u)-[:blocks]->(z) and not (z)-[:blocks]->(u)")
                .Return(z => z.As<UserNode>().UserId)
                .ResultsAsync;
        }
    }
}