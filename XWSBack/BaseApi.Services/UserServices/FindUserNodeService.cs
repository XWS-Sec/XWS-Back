using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseApi.Model.Graph;
using Neo4jClient;

namespace BaseApi.Services.UserServices
{
    public class FindUserNodeService
    {
        private readonly IGraphClient _client;

        public FindUserNodeService(IGraphClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<UserNode>> FindUser(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Return(u => u.As<UserNode>())
                .ResultsAsync;
        }
    }
}