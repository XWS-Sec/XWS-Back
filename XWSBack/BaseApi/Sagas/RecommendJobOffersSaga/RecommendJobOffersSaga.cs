using AutoMapper;
using BaseApi.Messages;
using BaseApi.Messages.Dtos;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using JobOffers.Messages;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.Graph.Messages.Skills;

namespace BaseApi.Sagas.RecommendJobOffersSaga
{
    public class RecommendJobOffersSaga : Saga<RecommendJobOffersSagaData>,
        IAmStartedByMessages<BeginJobOffersRecommendation>,
        IHandleMessages<GetSkillsResponse>,
        IHandleMessages<GetRecommendedJobOffersResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public RecommendJobOffersSaga(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<RecommendJobOffersSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginJobOffersRecommendation>(m => m.CorrelationId);
        }


        public async Task Handle(BeginJobOffersRecommendation message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;

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
                LinkName = "hasSkill",
                UserId = Data.UserId
            }).ConfigureAwait(false);

            await context.Send(new GetSkillsRequest()
            {
                CorrelationId = Data.CorrelationId,
                LinkName = "hasInterest",
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }


        public async Task Handle(GetSkillsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }
            
            if (Data.Interests == null) 
            {
                Data.Interests = message.Links;
                return;
            }

            Data.Interests = Data.Interests.Concat(message.Links);

            await context.Send(new GetRecommendedJobOffersRequest()
            {
                CorrelationId = Data.CorrelationId,
                Interests = Data.Interests.ToList()
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetRecommendedJobOffersResponse message, IMessageHandlerContext context)
        {
            await context.SendLocal(new GetJobOffersNotification()
            {
                CorrelationId = Data.CorrelationId,
                JobOffers = _mapper.Map<IEnumerable<JobOfferNotificationDto>>(message.JobOffers)
            }).ConfigureAwait(false);
            MarkAsComplete();
        }




        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }


        private async Task<string> Validate(BeginJobOffersRecommendation message)
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
