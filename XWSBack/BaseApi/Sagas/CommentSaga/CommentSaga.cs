using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using NServiceBus;
using Posts.Messages;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.CommentSaga
{
    public class CommentSaga : Saga<CommentSagaData>,
        IAmStartedByMessages<BeginCommentRequest>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleMessages<CommentResponse>,
        IHandleMessages<GetUserByPostResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public CommentSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CommentSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginCommentRequest>(m => m.CorrelationId)
                .ToMessage<GetFollowStatsResponse>(m => m.CorrelationId)
                .ToMessage<CommentResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginCommentRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.Text = message.Text;
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
                await FailSaga(context, "Post not found\n");
                return;
            }

            Data.PostOwnerId = message.PostOwnerId;
            
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
                await FailSaga(context, message.MessageToLog);
                return;
            }

            if (message.Blocked.Contains(Data.PostOwnerId))
            {
                await FailSaga(context, "You block the owner of that post");
                return;
            }

            if (message.BlockedFrom.Contains(Data.PostOwnerId))
            {
                await FailSaga(context, "The owner of the post is blocking you");
                return;
            }
            
            var postOwner = await _userManager.FindByIdAsync(Data.PostOwnerId.ToString());

            if (postOwner.IsPrivate)
            {
                if (message.Following.FirstOrDefault(x => x == Data.PostOwnerId) == Guid.Empty)
                {
                    await FailSaga(context, "You do not follow that user");
                    return;
                }
            }
            
            await context.Send(new CommentRequest()
            {
                Text = Data.Text,
                CorrelationId = Data.CorrelationId,
                PostId = Data.PostId,
                UserId = Data.UserId
            }).ConfigureAwait(false);
        }

        public async Task Handle(CommentResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, "Post not found");
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = JsonConvert.SerializeObject(message.CreatedComment),
                CorrelationId = Data.CorrelationId,
                IsSuccessful = true,
                UserId = Guid.NewGuid()
            }).ConfigureAwait(false);
            MarkAsComplete();
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout");
        }
        
        private async Task FailSaga(IMessageHandlerContext context, string reason)
        {
            await context.SendLocal(new StandardNotification()
            {
                Message = reason,
                UserId = Guid.NewGuid(),
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }

        private async Task<string> ValidateMessage(BeginCommentRequest request)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(request.Text))
            {
                retVal += "Text is mandatory\n";
            }

            if (request.PostId == Guid.Empty)
            {
                retVal += "Post is mandatory\n";
            }

            if (request.UserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }
            else
            {
                if (await _userManager.FindByIdAsync(request.UserId.ToString()) == null)
                {
                    retVal += "User not found\n";
                }
            }

            return retVal;
        }
    }
}