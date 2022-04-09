﻿using System;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using Neo4jClient;
using NServiceBus;
using Services.PictureServices;

namespace Services.UserServices
{
    public class CreateUserNodeService
    {
        private readonly IGraphClient _client;

        public CreateUserNodeService(IGraphClient client)
        {
            _client = client;
        }

        public async Task CreateNode(Guid id)
        {
            var newUser = new UserNode()
            {
                Id = id
            };

            await _client.Cypher.Create("(u:UserNode $user)")
                .WithParam("user", newUser)
                .ExecuteWithoutResultsAsync();
        }
    }
}