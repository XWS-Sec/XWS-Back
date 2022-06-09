﻿using System;
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

        public async Task<IEnumerable<Guid>> GetFollowing(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match("(u)-[f:follows]->(k)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }
        
        public async Task<IEnumerable<Guid>> GetFollowers(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match("(k)-[f:follows]->(u)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }

        public async Task<IEnumerable<Guid>> GetFollowRequests(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match("(k)-[f:followRequest]->(u)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }

        public async Task<IEnumerable<Guid>> GetFollowRequested(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Match("(u)-[f:followRequest]->(k)")
                .Return(k => k.As<UserNode>().UserId)
                .ResultsAsync;
        }
    }
}