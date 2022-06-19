using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Skills;

namespace BaseApi.Sagas.AdjustSkillsSaga
{
    public class AdjustSkillsSaga : Saga<AdjustSkillsSagaData>,
        IAmStartedByMessages<BeginAdjustSkillsRequest>,
        IHandleMessages<AdjustSkillsResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public AdjustSkillsSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }    
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AdjustSkillsSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginAdjustSkillsRequest>(m => m.CorrelationId);
        }

        public async Task Handle(BeginAdjustSkillsRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.LinkName = message.LinkName;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout()).ConfigureAwait(false);
            
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            await context.Send(new AdjustSkillsRequest()
            {
                CorrelationId = Data.CorrelationId,
                LinkName = message.LinkName,
                NewSkills = message.NewSkills,
                SkillsToRemove = message.SkillsToRemove,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }
        
        public async Task Handle(AdjustSkillsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = $"Successful adjusting of {(Data.LinkName.EndsWith("Interest") ? "interests" : "skills")}!",
                UserId = Data.UserId,
                IsSuccessful = true,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }
        
        private async Task<string> Validate(BeginAdjustSkillsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"User is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += $"User with id {message.UserId} not found\n";
            }

            if (message.NewSkills == null && message.SkillsToRemove == null)
            {
                retVal += $"Atleast one of the lists has to have values\n";
            }

            return retVal;
        }
        
        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Data.UserId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}