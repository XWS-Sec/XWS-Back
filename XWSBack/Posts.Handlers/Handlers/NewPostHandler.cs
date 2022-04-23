using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Model;

namespace Posts.Handlers.Handlers
{
    public class NewPostHandler : IHandleMessages<NewPostRequest>
    {
        private readonly IMongoCollection<Post> _postCollection;

        public NewPostHandler(IMongoClient client)
        {
            _postCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }
        
        public async Task Handle(NewPostRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new NewPostResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus,
                }).ConfigureAwait(false);
                return;
            }

            await _postCollection.InsertOneAsync(new Post()
            {
                Id = message.PostId,
                Text = message.Text,
                DateCreated = DateTime.Now,
                HasPicture = message.HasPicture,
                PosterId = message.UserId
            });

            await context.Reply(new NewPostResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successful"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(NewPostRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory\n";
            }

            if (message.PostId == Guid.Empty)
            {
                retVal += $"PostId is mandatory\n";
            }
            else
            {
                var post = await _postCollection.FindAsync(x => x.Id == message.PostId);
                if (await post.AnyAsync())
                {
                    retVal += $"Post with id {message.PostId} already exists\n";   
                }
            }
            
            return retVal;
        }
    }
}