using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BaseApi.Messages;
using BaseApi.Messages.Dtos;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Posts.Messages;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.GetPostsSaga
{
    public class GetPostsSaga : Saga<GetPostsSagaData>,
        IAmStartedByMessages<BeginGetPostsRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleMessages<GetPostsResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetPostsSaga(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetPostsSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginGetPostsRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetFollowStatsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetPostsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginGetPostsRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.RequestedUserId = message.RequestedUserId;
            Data.Page = message.Page;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout()).ConfigureAwait(false);
            
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            if (message.RequestedUserId != Guid.Empty)
            {
                var requestedUser = await _userManager.FindByIdAsync(Data.RequestedUserId.ToString());

                if (Data.RequestedUserId == Data.UserId || !requestedUser.IsPrivate)
                {
                    await context.Send(new GetPostsRequest()
                    {
                        Page = Data.Page,
                        CorrelationId = Data.CorrelationId,
                        PostsOwners = new List<Guid>() { Data.RequestedUserId }
                    }).ConfigureAwait(false);
                    return;
                }
            }

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }

        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            if (Data.RequestedUserId != Guid.Empty)
            {
                if (message.Following.Contains(Data.RequestedUserId))
                {
                    await context.Send(new GetPostsRequest()
                    {
                        Page = Data.Page,
                        CorrelationId = Data.CorrelationId,
                        PostsOwners = new List<Guid>() { Data.RequestedUserId }
                    }).ConfigureAwait(false);
                    return;
                }
                
                await FailSaga(context, "User is private and not followed").ConfigureAwait(false);
                return;
            }

            await context.Send(new GetPostsRequest()
            {
                Page = Data.Page,
                CorrelationId = Data.CorrelationId,
                PostsOwners = message.Following
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetPostsResponse message, IMessageHandlerContext context)
        {
            await context.SendLocal(new PostsNotification()
            {
                UserId = Data.UserId,
                Posts = _mapper.Map<List<PostNotificationDto>>(message.Posts),
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
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
        
        private async Task<string> Validate(BeginGetPostsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId != Guid.Empty && await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += $"User with id {message.UserId} not found\n";
            }

            if (message.UserId == Guid.Empty && message.RequestedUserId == Guid.Empty)
            {
                retVal += $"You cannot get posts if you are not logged in, only of a specific user who has to have a public profile\n";
            }
            
            if (message.RequestedUserId != Guid.Empty)
            {
                var requestedUser = await _userManager.FindByIdAsync(message.RequestedUserId.ToString());
                if (requestedUser == null)
                {
                    retVal += $"User with id {message.RequestedUserId} not found\n";
                }else if (message.UserId == Guid.Empty && requestedUser.IsPrivate)
                {
                    retVal += $"Cannot retrieve posts of a private user if you are not logged in\n";
                }
            }

            return retVal;
        }
    }
}