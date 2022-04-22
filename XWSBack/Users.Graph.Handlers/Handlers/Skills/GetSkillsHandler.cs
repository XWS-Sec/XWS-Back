using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Skills;

namespace Users.Graph.Handlers.Handlers.Skills
{
    public class GetSkillsHandler : IHandleMessages<GetSkillsRequest>
    {
        private readonly UserNodeService _userNodeService;
        private readonly SkillManagementService _skillManagementService;

        public GetSkillsHandler(UserNodeService userNodeService, SkillManagementService skillManagementService)
        {
            _userNodeService = userNodeService;
            _skillManagementService = skillManagementService;
        }
        
        public async Task Handle(GetSkillsRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new GetSkillsResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var tags = await _skillManagementService.GetSkills(message.UserId, message.LinkName);
            await context.Reply(new GetSkillsResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                Links = tags.Select(x => x.Name).ToList(),
                MessageToLog = "Successful"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(GetSkillsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.UserId))
            {
                retVal += $"User with id {message.CorrelationId} not found\n";
            }

            if (message.LinkName != "hasInterest" && message.LinkName != "hasSkill")
            {
                retVal += $"Unknown link name {message.LinkName}\n";
            }

            return retVal;
        }
    }
}