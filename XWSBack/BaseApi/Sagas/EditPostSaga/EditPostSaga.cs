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

namespace BaseApi.Sagas.EditPostSaga
{
    public class EditPostSaga : Saga<EditPostSagaData>,
        IAmStartedByMessages<BeginEditPostRequest>,
        IHandleTimeouts<BaseTimeout>,
        IHandleMessages<EditPostResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly PictureService _pictureService;

        public EditPostSaga(UserManager<User> userManager, PictureService pictureService)
        {
            _userManager = userManager;
            _pictureService = pictureService;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<EditPostSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginEditPostRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<EditPostResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginEditPostRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.PostId = message.PostId;

            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus).ConfigureAwait(false);
                return;
            }

            if (message.Picture != null && message.Picture.Length > 0)
            {
                Data.InternalPictureId = Guid.NewGuid();
                _pictureService.SavePostPicture(Data.InternalPictureId, message.Picture);
            }

            await context.Send(new EditPostRequest()
            {
                Text = message.Text,
                HasPicture = message.Picture != null && message.Picture.Length > 0,
                CorrelationId = Data.CorrelationId,
                PostId = Data.PostId,
                UserId = Data.UserId,
                RemoveOldPic = message.RemoveOldPic
            }).ConfigureAwait(false);
        }

        public async Task Timeout(BaseTimeout state, IMessageHandlerContext context)
        {
            await FailSaga(context, "Timeout").ConfigureAwait(false);
        }

        public async Task Handle(EditPostResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                if (Data.InternalPictureId != Guid.Empty)
                {
                    _pictureService.DeletePostPicture(Data.InternalPictureId);
                }

                await FailSaga(context, message.MessageToLog).ConfigureAwait(false);
                return;
            }

            if (message.ShouldDeleteOldPicture)
            {
                _pictureService.DeletePostPicture(Data.PostId);
            }

            if (Data.InternalPictureId != Guid.Empty)
            {
                _pictureService.ChangePostPictureName(Data.InternalPictureId, Data.PostId);
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = $"Successfully edited post {Data.PostId}",
                IsSuccessful = true,
                UserId = Data.UserId,
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
        
        private async Task<string> Validate(BeginEditPostRequest message)
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
    }
}