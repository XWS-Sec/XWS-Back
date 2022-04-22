using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Services
{
    public class SkillManagementService
    {
        private readonly IGraphClient _client;

        public SkillManagementService(IGraphClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<SkillTag>> GetSkills(Guid id, string linkName)
        {
            return await _client.Cypher.Match("(u:UserNode), (s:SkillTag)")
                .Where((UserNode u) => u.UserId == id)
                .Match($"(u)-[h:{linkName}]->(s)")
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        }

        public async Task AdjustSkills(Guid userId, IEnumerable<string> newSkills, IEnumerable<string> skillsToRemove, string link)
        {
            if (newSkills != null)
                foreach (var skill in newSkills)
                {
                    await AddNewSkill(userId, skill.ToLower(), link);
                }
            
            if (skillsToRemove != null)
                foreach (var skill in skillsToRemove)
                {
                    await RemoveSkill(userId, skill.ToLower(), link);
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
            var tag = await FindTag(name);

            if (!tag.Any()) await CreateTag(name);

            var linkedTag = await LinkedTag(userId, name, linkName);
            if (!linkedTag.Any()) await CreateLink(userId, name, linkName);
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
                .WithParam("param", new SkillTag
                {
                    Id = Guid.NewGuid(),
                    Name = name
                }).ExecuteWithoutResultsAsync();
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