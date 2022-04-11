using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using Neo4jClient;

namespace Services.SkillsServices
{
    public class SkillManagementService
    {
        private readonly IGraphClient _client;

        public SkillManagementService(IGraphClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<SkillTag>> GetSkills(Guid id)
        {
            return await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u) => u.UserId == id)
                .Match("(u)-[h:hasSkill]->(s)")
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        }
        
        public async Task Handle(Guid userId, IEnumerable<string> newSkills, IEnumerable<string> skillsToRemove)
        {
            var foundUsers = await FindUser(userId);

            if (!foundUsers.Any())
            {
                throw new Exception($"UserNode with {userId} does not exist");
            }

            if (newSkills != null)
            {
                foreach (var skill in newSkills)
                {
                    await AddNewSkill(userId, skill.ToLower());
                }   
            }

            if (skillsToRemove != null)
            {
                foreach (var skill in skillsToRemove)
                {
                    await RemoveSkill(userId, skill.ToLower());
                }   
            }
        }

        private async Task RemoveSkill(Guid userId, string name)
        {
            await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Match("(u)-[h:hasSkill]->(s)")
                .Delete("h")
                .ExecuteWithoutResultsAsync();
        }
        
        private async Task AddNewSkill(Guid userId, string name)
        {
            var tag = await FindTag(name);

            if (!tag.Any())
            {
                await CreateTag(name);
            }

            var linkedTag = await LinkedTag(userId, name);
            if (!linkedTag.Any())
            {
                await CreateLink(userId, name);
            }
        }

        private async Task CreateLink(Guid userId, string name)
        {
            await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Create("(u)-[h:hasSkill]->(s)")
                .ExecuteWithoutResultsAsync();
        }
        
        private async Task<IEnumerable<SkillTag>> LinkedTag(Guid userId, string name)
        {
            return await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u, SkillTag s) => u.UserId == userId && s.Name == name)
                .Match("(u)-[h:hasSkill]->(s)")
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
        
        private async Task<IEnumerable<UserNode>> FindUser(Guid userId)
        {
            return await _client.Cypher.Match("(u:UserNode)")
                .Where((UserNode u) => u.UserId == userId)
                .Return(u => u.As<UserNode>())
                .ResultsAsync;
        }

        private async Task<IEnumerable<SkillTag>> FindTag(string name)
        {
            return await _client.Cypher.Match("(s:SkillTag)")
                .Where((SkillTag s) => s.Name == name)
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        }
    }
}