using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chats.Messages;
using Chats.Model;
using MongoDB.Driver;
using NServiceBus;

namespace Chats.Handlers.Handlers
{
    public class AddMessageHandler : IHandleMessages<AddMessageRequest>
    {
        private readonly IMongoCollection<Chat> _chatsCollection;

        public AddMessageHandler(IMongoClient client)
        {
            _chatsCollection = client.GetDatabase("Chats").GetCollection<Chat>("Chats");
        }

        public async Task Handle(AddMessageRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new AddMessageResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var correspondingChat = await FindChat(message.SenderId, message.ReceiverId);

            if (correspondingChat.Messages == null)
            {
                correspondingChat.Messages = new List<Message>();
            }
            
            correspondingChat.Messages.Add(new Message()
            {
                Text = message.Message,
                DateCreated = message.DateCreated,
                SenderId = message.SenderId
            });

            await _chatsCollection.FindOneAndReplaceAsync(x => x.Id == correspondingChat.Id, correspondingChat);

            await context.Reply(new AddMessageResponse()
            {
                IsSuccessful = true,
                CorrelationId = message.CorrelationId,
                MessageToLog = "Success"
            }).ConfigureAwait(false);
        }

        private async Task<Chat> FindChat(Guid user1, Guid user2)
        {
            var filter = Builders<Chat>.Filter.Where(x => x.Members.Contains(user1) && x.Members.Contains(user2));

            var chats = await _chatsCollection.FindAsync(filter, new FindOptions<Chat>()
            {
                Limit = 1
            });

            var chat = chats.FirstOrDefault();

            if (chat is null)
            {
                chat = new Chat()
                {
                    Id = Guid.NewGuid(),
                    Members = new List<Guid>()
                    {
                        user1, user2
                    }
                };

                await _chatsCollection.InsertOneAsync(chat);
            }

            return chat;
        }
        
        private async Task<string> Validate(AddMessageRequest message)
        {
            var retVal = string.Empty;

            if (message.ReceiverId == Guid.Empty)
            {
                retVal += $"ReceiverId is mandatory\n";
            }

            if (message.SenderId == Guid.Empty)
            {
                retVal += $"SenderId is mandatory\n";
            }

            if (message.SenderId == message.ReceiverId)
            {
                retVal += $"Sender and receiver cannot  be the same person\n";
            }
            
            if (string.IsNullOrEmpty(message.Message))
            {
                retVal += $"Message cannot be empty\n";
            }

            return retVal;
        }
    }
}