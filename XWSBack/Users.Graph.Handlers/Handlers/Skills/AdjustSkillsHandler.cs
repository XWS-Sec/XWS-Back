using System;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Skills;

namespace Users.Graph.Handlers.Handlers.Skills
{
    public class AdjustSkillsHandler : IHandleMessages<AdjustSkillsRequest>
    {
        private readonly SkillManagementService _skillManagementService;
        private readonly UserNodeService _userNodeService;

        public AdjustSkillsHandler(SkillManagementService skillManagementService, UserNodeService userNodeService)
        {
            _skillManagementService = skillManagementService;
            _userNodeService = userNodeService;
        }
        
        public async Task Handle(AdjustSkillsRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new AdjustSkillsResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await _skillManagementService.AdjustSkills(message.UserId, message.NewSkills, message.SkillsToRemove, message.LinkName);

            await context.Reply(new AdjustSkillsResponse()
            {
                CorrelationId = message.CorrelationId,
                MessageToLog = $"Successful!",
                IsSuccessful = true
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(AdjustSkillsRequest message)
        {
            var retVal = string.Empty;

            if (message.LinkName != "hasSkill" && message.LinkName != "hasInterest")
            {
                retVal += $"Invalid link name {message.LinkName}\n";
            }

            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.UserId))
            {
                retVal += $"User with id {message.UserId} not found\n";
            }

            if (message.NewSkills == null && message.SkillsToRemove == null)
            {
                retVal += $"Atleast one of the lists has to have some values to adjust\n";
            }
            
            return retVal;
        }
    }
}