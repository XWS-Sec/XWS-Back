using System;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Services
{
    public class UserNodeService
    {
        private readonly IGraphClient _client;

        public UserNodeService(IGraphClient client)
        {
            _client = client;
        }

        public async Task<bool> UserExists(Guid id)
        {
            var existingUser = await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == id)
                .Return(u => u.As<UserNode>())
                .ResultsAsync;
            
            if (existingUser.Any())
                return true;
            return false;
        }
    }
}