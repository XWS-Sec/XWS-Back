using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Dtos;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.GetFollowStatsSaga
{
    public class GetFollowStatsSaga : Saga<GetFollowStatsSagaData>,
        IAmStartedByMessages<BeginGetFollowStatsRequest>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public GetFollowStatsSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetFollowStatsSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginGetFollowStatsRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetFollowStatsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginGetFollowStatsRequest message, IMessageHandlerContext context)
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

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }
        
        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            var followers = await FindUsers(message.Followers);
            var following = await FindUsers(message.Following);
            var followRequests = await FindUsers(message.FollowRequests);
            var followRequested = await FindUsers(message.FollowRequested);

            await context.SendLocal(new FollowStatsNotification()
            {
                UserId = Data.UserId,
                Followers = followers,
                Following = following,
                FollowRequests = followRequests,
                FollowRequested = followRequested,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            MarkAsComplete();
        }
        
        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout.").ConfigureAwait(false);
        }
        
        private async Task<string> Validate(BeginGetFollowStatsRequest message)
        {
            var retVal = string.Empty;
            
            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += $"User with id {message.UserId} not found \n";
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

        private async Task<List<UserNotificationDto>> FindUsers(IEnumerable<Guid> userIds)
        {
            var retVal = new List<UserNotificationDto>();
            foreach (var follower in userIds)
            {
                var currentUser = await _userManager.FindByIdAsync(follower.ToString());
                retVal.Add(new UserNotificationDto()
                {
                    Id = currentUser.Id,
                    Username = currentUser.UserName,
                    Name = currentUser.Name,
                    Surname = currentUser.Surname,
                    IsPrivate = currentUser.IsPrivate
                });
            }

            return retVal;
        }
    }
}