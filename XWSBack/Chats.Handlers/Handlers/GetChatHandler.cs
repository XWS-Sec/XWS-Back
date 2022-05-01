using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chats.Messages;
using Chats.Messages.Dtos;
using Chats.Model;
using MongoDB.Driver;
using NServiceBus;

namespace Chats.Handlers.Handlers
{
    public class GetChatHandler : IHandleMessages<GetChatRequest>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Chat> _chatCollection;
        private static readonly int pageSize = 10;

        public GetChatHandler(IMapper mapper, IMongoClient client)
        {
            _mapper = mapper;
            _chatCollection = client.GetDatabase("Chats").GetCollection<Chat>("Chats");
        }
        
        public async Task Handle(GetChatRequest message, IMessageHandlerContext context)
        {
            var validationStatus = Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new GetChatResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await context.Reply(new GetChatResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successful",
                Messages = await GetMessages(message)
            }).ConfigureAwait(false);
        }

        private string Validate(GetChatRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"User is mandatory\n";
            }

            if (message.OtherUserId == Guid.Empty)
            {
                retVal += $"Other user is mandatory\n";
            }

            if (message.UserId == message.OtherUserId)
            {
                retVal += $"User and otherUser cannot be the same person\n";
            }

            return retVal;
        }

        private async Task<IEnumerable<MessageDto>> GetMessages(GetChatRequest message)
        {
            await EnsureExists(message);

            var filter = Builders<Chat>.Filter.Where(x =>
                x.Members.Contains(message.UserId) && x.Members.Contains(message.OtherUserId));

            var messages = await _chatCollection.Find(filter)
                .Project(x => x.Messages
                    .Select(y => y)
                    .OrderByDescending(y => y.DateCreated)
                    .Skip(message.Page >= 0 ? message.Page * pageSize : 0)
                    .Take(pageSize))
                .ToCursorAsync();

            return _mapper.Map<List<MessageDto>>(messages.FirstOrDefault());;
        }

        private async Task EnsureExists(GetChatRequest message)
        {
            var existsFilter = Builders<Chat>.Filter.Where(x =>
                x.Members.Contains(message.UserId) && x.Members.Contains(message.OtherUserId));

            var existsResult = await _chatCollection.FindAsync(existsFilter, new FindOptions<Chat>()
            {
                Limit = 1,
            });

            if (!await existsResult.AnyAsync())
            {
                await _chatCollection.InsertOneAsync(new Chat()
                {
                    Id = Guid.NewGuid(),
                    Members = new List<Guid>()
                    {
                        message.UserId,
                        message.OtherUserId
                    },
                    Messages = new List<Message>(),
                });
            }
        }
    }
}