using System.Collections.Generic;
using System.Threading.Tasks;
using BaseApiModel.Graph;
using Neo4jClient;

namespace Services.SkillsServices
{
    public class FindSkillNodeService
    {
        private readonly IGraphClient _client;

        public FindSkillNodeService(IGraphClient client)
        {
            _client = client;
        }
        
        public async Task<IEnumerable<SkillTag>> FindTag(string name)
        {
            return await _client.Cypher.Match("(s:SkillTag)")
                .Where((SkillTag s) => s.Name == name)
                .Return(s => s.As<SkillTag>())
                .ResultsAsync;
        }
    }
}