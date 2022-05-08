using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using BaseApi.Services.PictureServices;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Posts.Messages;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.NewPostSaga
{
    public class NewPostSaga : Saga<NewPostSagaData>,
        IAmStartedByMessages<BeginNewPostRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<NewPostResponse>,
        IHandleMessages<GetFollowStatsResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly PictureService _pictureService;

        public NewPostSaga(UserManager<User> userManager, PictureService pictureService)
        {
            _userManager = userManager;
            _pictureService = pictureService;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<NewPostSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginNewPostRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<NewPostResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetFollowStatsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginNewPostRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.PostId = Guid.NewGuid();
            Data.HasPicture = message.Picture != null && message.Picture.Length > 0;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout()).ConfigureAwait(false);

            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            if (Data.HasPicture)
            {
                _pictureService.SavePostPicture(Data.PostId, message.Picture);
            }

            await context.Send(new NewPostRequest()
            {
                Text = message.Text,
                HasPicture = Data.HasPicture,
                PostId = Data.PostId,
                UserId = Data.UserId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }

        public async Task Handle(NewPostResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                if (Data.HasPicture)
                {
                    _pictureService.DeletePostPicture(Data.PostId);
                }

                await FailSaga(context, "Failed to create new post").ConfigureAwait(false);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = $"Successfully created a new post with id {Data.PostId}",
                IsSuccessful = true,
                UserId = Data.UserId
            }).ConfigureAwait(false);

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

            await context.SendLocal(new NewPostNotification()
            {
                Poster = Data.UserId,
                UsersToNotify = message.Followers
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        private async Task<string> Validate(BeginNewPostRequest message)
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
                UserId = Data.UserId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
    }
}