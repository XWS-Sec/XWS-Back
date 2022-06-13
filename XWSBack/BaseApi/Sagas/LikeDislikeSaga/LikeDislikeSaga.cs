using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Posts.Messages;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.LikeDislikeSaga
{
    public class LikeDislikeSaga : Saga<LikeDislikeSagaData>,
        IAmStartedByMessages<BeginLikeDislikeRequest>,
        IHandleMessages<GetUserByPostResponse>,
        IHandleMessages<LikeDislikeResponse>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public LikeDislikeSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<LikeDislikeSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginLikeDislikeRequest>(m => m.CorrelationId)
                .ToMessage<GetUserByPostResponse>(m => m.CorrelationId)
                .ToMessage<GetFollowStatsResponse>(m => m.CorrelationId)
                .ToMessage<LikeDislikeResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginLikeDislikeRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.IsLike = message.IsLike;
            Data.PostId = message.PostId;

            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new GetUserByPostRequest()
            {
                CorrelationId = Data.CorrelationId,
                PostId = Data.PostId
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetUserByPostResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, "Post not found");
                return;
            }

            Data.PostOwnerId = message.PostOwnerId;

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Handle(LikeDislikeResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = "Success",
                CorrelationId = Data.CorrelationId,
                IsSuccessful = true,
                UserId = Guid.NewGuid()
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }

        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            if (message.Blocked.Contains(Data.PostOwnerId))
            {
                await FailSaga(context, "You are blocking that user");
                return;
            }

            if (message.BlockedFrom.Contains(Data.PostOwnerId))
            {
                await FailSaga(context, "Post owner is blocking you");
                return;
            }

            var postOwner = await _userManager.FindByIdAsync(Data.PostOwnerId.ToString());

            if (postOwner.IsPrivate && !message.Following.Contains(Data.PostOwnerId))
            {
                await FailSaga(context, "You dont follow the post owner");
                return;
            }
            
            await context.Send(new LikeDislikeRequest()
            {
                CorrelationId = Data.CorrelationId,
                IsLike = Data.IsLike,
                PostId = Data.PostId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }
        
        private async Task<string> ValidateMessage(BeginLikeDislikeRequest message)
        {
            var retVal = string.Empty;

            if (message.PostId == Guid.Empty)
            {
                retVal += "Post is mandatory\n";
            }

            if (message.UserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }
            else
            {
                if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
                {
                    retVal += "User not found\n";
                }
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