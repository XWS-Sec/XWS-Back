using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using Neo4jClient;
using Services.UserServices;

namespace Services.SkillsServices
{
    public class SkillManagementService
    {
        private readonly IGraphClient _client;
        private readonly FindUserNodeService _findUserNodeService;
        private readonly FindSkillNodeService _findSkillNodeService;

        public SkillManagementService(IGraphClient client, FindUserNodeService findUserNodeService, FindSkillNodeService findSkillNodeService)
        {
            _client = client;
            _findUserNodeService = findUserNodeService;
            _findSkillNodeService = findSkillNodeService;
        }

        public async Task<IEnumerable<SkillTag>> GetSkills(Guid id, string linkName)
        {
            return await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u) => u.UserId == id)
                .Match($"(u)-[h:{linkName}]->(s)")
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        }
        
        public async Task Handle(Guid userId, IEnumerable<string> newSkills, IEnumerable<string> skillsToRemove, string link)
        {
            var foundUsers = await _findUserNodeService.FindUser(userId);

            if (!foundUsers.Any())
            {
                throw new Exception($"UserNode with {userId} does not exist");
            }

            if (newSkills != null)
            {
                foreach (var skill in newSkills)
                {
                    await AddNewSkill(userId, skill.ToLower(), link);
                }   
            }

            if (skillsToRemove != null)
            {
                foreach (var skill in skillsToRemove)
                {
                    await RemoveSkill(userId, skill.ToLower(), link);
                }   
            }
        }

        private async Task RemoveSkill(Guid userId, string name, string linkName)
        {
            await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Match($"(u)-[h:{linkName}]->(s)")
                .Delete("h")
                .ExecuteWithoutResultsAsync();
        }
        
        private async Task AddNewSkill(Guid userId, string name, string linkName)
        {
            var tag = await _findSkillNodeService.FindTag(name);

            if (!tag.Any())
            {
                await CreateTag(name);
            }

            var linkedTag = await LinkedTag(userId, name, linkName);
            if (!linkedTag.Any())
            {
                await CreateLink(userId, name, linkName);
            }
        }

        private async Task CreateLink(Guid userId, string name, string linkName)
        {
            await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Create($"(u)-[h:{linkName}]->(s)")
                .ExecuteWithoutResultsAsync();
        }
        
        private async Task<IEnumerable<SkillTag>> LinkedTag(Guid userId, string name, string linkName)
        {
            return await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Match($"(u)-[h:{linkName}]->(s)")
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        } 

        private async Task CreateTag(string name)
        {
            await _client.Cypher.Create("(s:SkillTag $param)")
                .WithParam("param", new SkillTag()
                {
                    Id = Guid.NewGuid(),
                    Name = name
                }).ExecuteWithoutResultsAsync();
        }
        
    }
}