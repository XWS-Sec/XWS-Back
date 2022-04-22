using System.Collections.Generic;

namespace BaseApi.Dto.Skills
{
    public class AdjustSkillsDto
    {
        public IEnumerable<string> NewSkills { get; set; }
        public IEnumerable<string> SkillsToRemove { get; set; }
    }
}