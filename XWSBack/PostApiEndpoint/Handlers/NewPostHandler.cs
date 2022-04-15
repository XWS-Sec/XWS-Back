using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using PostMessages;
using PostServiceModel;

namespace PostApiEndpoint.Handlers
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
            var response = new NewPostResponse()
            {
                PostId = message.PostId,
                IsSuccessful = false
            };
            try
            {
                await _postCollection.InsertOneAsync(new Post()
                {
                    Id = message.PostId,
                    PosterId = message.UserId,
                    Text = message.Text,
                    HasPicture = message.HasPicture
                });
            }
            catch (Exception e)
            {
                await context.Reply(response).ConfigureAwait(false);
                return;
            }

            response.IsSuccessful = true;
            await context.Reply(response).ConfigureAwait(false);
        }
    }
}