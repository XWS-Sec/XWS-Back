using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Skills;

namespace BaseApi.Sagas.GetSkillsSaga
{
    public class GetSkillsSaga : Saga<GetSkillsSagaData>,
        IAmStartedByMessages<BeginGetSkillsRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<GetSkillsResponse>
    {
        private readonly UserManager<User> _userManager;

        public GetSkillsSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetSkillsSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginGetSkillsRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetSkillsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginGetSkillsRequest message, IMessageHandlerContext context)
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

            await context.Send(new GetSkillsRequest()
            {
                CorrelationId = Data.CorrelationId,
                LinkName = Data.LinkName,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }
        
        public async Task Handle(GetSkillsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new SkillStatsNotification()
            {
                IsSuccessful = true,
                LinkName = Data.LinkName,
                Skills = message.Links,
                UserId = Data.UserId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        private async Task<string> Validate(BeginGetSkillsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += $"User with id {message.UserId} not found\n";
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