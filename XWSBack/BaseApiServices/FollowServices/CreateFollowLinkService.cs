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
    public class CreateFollowLinkService
    {
        private readonly IGraphClient _graphClient;
        private readonly UserManager<User> _userManager;

        public CreateFollowLinkService(IGraphClient graphClient, UserManager<User> userManager)
        {
            _graphClient = graphClient;
            _userManager = userManager;
        }

        public async Task Create(Guid sender, Guid receiver)
        {
            if (sender == receiver)
                throw new Exception($"User cannot follow himself");
            
            var receiverUser = await ValidateIfExists(receiver);
            var senderUser = await ValidateIfExists(sender);
            
            CheckIfBlocked(senderUser, receiverUser);
            await LinkExists(senderUser, receiverUser);
            
            await CreateLink(senderUser, receiverUser);
        }

        private async Task<User> ValidateIfExists(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString())
                .ConfigureAwait(false);

            if (user == null)
                throw new Exception($"User with id {id} is not present in the database");

            var userNode = await _graphClient.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == id)
                .Return(u => u.As<UserNode>())
                .ResultsAsync;

            var foundNode = userNode.FirstOrDefault();
            if (foundNode == null)
                throw new Exception($"User with id {id} is not present in the graph");

            return user;
        }

        private void CheckIfBlocked(User sender, User receiver)
        {
            if (sender.BlockedUsers != null && sender.BlockedUsers.Contains(receiver.Id))
                throw new Exception($"User {receiver.UserName} is blocked by {sender.UserName}");

            if (receiver.BlockedUsers != null && receiver.BlockedUsers.Contains(sender.Id))
                throw new Exception($"User {sender.UserName} is blocked by {receiver.UserName}");
        }

        private async Task LinkExists(User sender, User receiver)
        {
            var requestLink = await FindLink<FollowRequest>(sender, receiver, "followRequest");
            if (requestLink.FirstOrDefault() != null)
                throw new Exception($"User {sender.UserName} already requested following the user {receiver.UserName}");

            var followLink = await FindLink<Follow>(sender, receiver, "follows");
            var link = followLink.FirstOrDefault();
            if (link != null)
                throw new Exception($"User {sender.UserName} already follows the user {receiver.UserName} from {link.CreatedDate.ToString("d")}");
        }

        private async Task<IEnumerable<T>> FindLink<T>(User sender, User receiver, string linkName)
        {
            return await _graphClient.Cypher.Match("(s:UserNode), (r:UserNode)")
                .Where((UserNode s, UserNode r) => s.UserId == sender.Id && r.UserId == receiver.Id)
                .Match($"(s)-[f:{linkName}]->(r)")
                .Return(f => f.As<T>())
                .ResultsAsync;
        }

        private async Task CreateLink(User sender, User receiver)
        {
            if (receiver.IsPrivate)
            {
                var followRequest = new FollowRequest()
                {
                    FollowRequestId = Guid.NewGuid(),
                    CreatedDate = DateTime.Now
                };
                await CreateLink(sender, receiver, followRequest, "followRequest");
            }
            else
            {
                var follow = new Follow()
                {
                    FollowId = Guid.NewGuid(),
                    CreatedDate = DateTime.Now
                };
                await CreateLink(sender, receiver, follow, "follows");
            }
        }

        private async Task CreateLink<T>(User sender, User receiver, T link, string linkName)
        {
            await _graphClient.Cypher.Match("(s:UserNode), (r:UserNode)")
                .Where((UserNode s, UserNode r) => s.UserId == sender.Id && r.UserId == receiver.Id)
                .Create($"(s)-[l:{linkName} $req]->(r)")
                .WithParam("req", link)
                .ExecuteWithoutResultsAsync();
        }
    }
}