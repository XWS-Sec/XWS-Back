using System;
using System.Threading.Tasks;
using BaseApi.Messages;
using BaseApi.Messages.Notifications;
using BaseApi.Messages.Timeouts;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.BlockUnblockSaga
{
    public class BlockUnblockSaga : Saga<BlockUnblockSagaData>,
        IAmStartedByMessages<BeginBlockUnblockRequest>,
        IHandleMessages<BlockResponse>,
        IHandleMessages<UnblockResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;

        public BlockUnblockSaga(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BlockUnblockSagaData> mapper)
        {
            mapper.MapSaga(s => s.CorrelationId)
                .ToMessage<BeginBlockUnblockRequest>(m => m.CorrelationId)
                .ToMessage<BlockResponse>(m => m.CorrelationId)
                .ToMessage<UnblockResponse>(m => m.CorrelationId);
        }

        public async Task Handle(BeginBlockUnblockRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.OtherUserId = message.OtherUserId;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());

            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            if (message.IsBlock)
            {
                await context.Send(new BlockRequest()
                {
                    CorrelationId = Data.CorrelationId,
                    RequesterId = message.UserId,
                    UserToBlockId = message.OtherUserId
                }).ConfigureAwait(false);
            }
            else
            {
                await context.Send(new UnblockRequest()
                {
                    CorrelationId = Data.CorrelationId,
                    RequesterId = message.UserId,
                    BlockedUserId = message.OtherUserId
                }).ConfigureAwait(false);
            }
        }

        public async Task Handle(BlockResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = "Successfully blocked user",
                CorrelationId = Data.CorrelationId,
                IsSuccessful = true,
                UserId = Guid.NewGuid()
            }).ConfigureAwait(false);
            MarkAsComplete();
        }

        public async Task Handle(UnblockResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new StandardNotification()
            {
                Message = "Successfully unblocked user",
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

        private async Task<string> ValidateMessage(BeginBlockUnblockRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.UserId.ToString()) == null)
            {
                retVal += "User not found\n";
            }

            if (message.OtherUserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.OtherUserId.ToString()) == null)
            {
                retVal += "User not found\n";
            }

            if (message.UserId == message.OtherUserId)
            {
                retVal += "User cannot block or unblock himself\n";
            }

            return retVal;
        }
    }
}