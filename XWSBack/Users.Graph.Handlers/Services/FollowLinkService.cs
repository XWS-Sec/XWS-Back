using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Services
{
    public class FollowLinkService
    {
        private readonly IGraphClient _client;

        public FollowLinkService(IGraphClient client)
        {
            _client = client;
        }
        
        public async Task<bool> HasLink<T>(Guid sender, Guid receiver, string linkName)
        {
            var link = await FindLink<T>(sender, receiver, linkName);
            if (link.Any())
                return true;
            return false;
        }
        
        private async Task<IEnumerable<T>> FindLink<T>(Guid sender, Guid receiver, string linkName)
        {
            return await _client.Cypher.Match("(s:UserNode), (r:UserNode)")
                .Where((UserNode s, UserNode r) => s.UserId == sender && r.UserId == receiver)
                .Match($"(s)-[f:{linkName}]->(r)")
                .Return(f => f.As<T>())
                .ResultsAsync;
        }
        
        public async Task CreateLinkBetweenUsers<T>(Guid sender, Guid receiver, string linkName, T data)
        {
            await _client.Cypher.Match("(s:UserNode), (r:UserNode)")
                .Where((UserNode s, UserNode r) => s.UserId == sender && r.UserId == receiver)
                .Create($"(s)-[l:{linkName} $param]->(r)")
                .WithParam("param", data)
                .ExecuteWithoutResultsAsync();
        }

        public async Task DeleteLinkBetweenUsers(Guid sender, Guid receiver, string linkName)
        {
            await _client.Cypher.Match("(s:UserNode), (r:UserNode)")
                .Where((UserNode s, UserNode r) => s.UserId == sender && r.UserId == receiver)
                .Match($"(s)-[l:{linkName}]->(r)")
                .Delete("l")
                .ExecuteWithoutResultsAsync();
        }
    }
}