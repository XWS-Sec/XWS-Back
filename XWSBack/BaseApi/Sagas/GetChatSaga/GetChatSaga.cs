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
using Chats.Messages;
using Microsoft.AspNetCore.Identity;
using NServiceBus;
using Users.Graph.Messages.Follow;

namespace BaseApi.Sagas.GetChatSaga
{
    public class GetChatSaga : Saga<GetChatSagaData>,
        IAmStartedByMessages<BeginGetChatRequest>,
        IHandleMessages<GetFollowStatsResponse>,
        IHandleMessages<GetChatResponse>,
        IHandleTimeouts<BaseTimeout>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetChatSaga(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }
        
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GetChatSagaData> mapper)
        {
            mapper.ConfigureMapping<BeginGetChatRequest>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetFollowStatsResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<GetChatResponse>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public async Task Handle(BeginGetChatRequest message, IMessageHandlerContext context)
        {
            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.OtherUserId = message.OtherUserId;
            Data.Page = message.Page;

            await RequestTimeout(context, TimeSpan.FromMinutes(5), new BaseTimeout());
            
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await FailSaga(context, validationStatus);
                return;
            }

            await context.Send(new GetFollowStatsRequest()
            {
                CorrelationId = Data.CorrelationId,
                UserId = message.UserId
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetFollowStatsResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            if (!message.Followers.Contains(Data.OtherUserId) || !message.Following.Contains(Data.OtherUserId))
            {
                await FailSaga(context, $"The users {Data.UserId} and {Data.OtherUserId} do not follow each other");
                return;
            }

            await context.Send(new GetChatRequest()
            {
                Page = Data.Page,
                CorrelationId = Data.CorrelationId,
                UserId = Data.UserId,
                OtherUserId = Data.OtherUserId
            }).ConfigureAwait(false);
        }

        public async Task Handle(GetChatResponse message, IMessageHandlerContext context)
        {
            if (!message.IsSuccessful)
            {
                await FailSaga(context, message.MessageToLog);
                return;
            }

            await context.SendLocal(new GetChatNotification()
            {
                Messages = _mapper.Map<List<MessageNotificationDto>>(message.Messages),
                UserId = Data.UserId,
                OtherUserId = Data.OtherUserId,
                CorrelationId = Data.CorrelationId
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
                UserId = Data.UserId,
                CorrelationId = Data.CorrelationId
            }).ConfigureAwait(false);
            
            MarkAsComplete();
        }
        
        private async Task<string> Validate(BeginGetChatRequest message)
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

            if (message.OtherUserId == Guid.Empty)
            {
                retVal += $"OtherUserId is mandatory\n";
            }
            else if (await _userManager.FindByIdAsync(message.OtherUserId.ToString()) == null)
            {
                retVal += $"User with id {message.OtherUserId} not found \n";
            }

            if (message.UserId == message.OtherUserId)
            {
                retVal += $"User cannot have chat with himself\n";
            }
            
            return retVal;
        }
    }
}