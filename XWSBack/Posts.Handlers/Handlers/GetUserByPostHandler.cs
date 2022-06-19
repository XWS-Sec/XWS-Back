using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Model;

namespace Posts.Handlers.Handlers
{
    public class GetUserByPostHandler : IHandleMessages<GetUserByPostRequest>
    {
        private readonly IMongoCollection<Post> _postCollection;

        public GetUserByPostHandler(IMongoClient client)
        {
            _postCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }

        public async Task Handle(GetUserByPostRequest message, IMessageHandlerContext context)
        {
            var postCursor = await _postCollection.FindAsync(x => x.Id == message.PostId);
            var post = postCursor.FirstOrDefault();

            await context.Reply(new GetUserByPostResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = post != null,
                PostOwnerId = post?.PosterId ?? Guid.Empty
            }).ConfigureAwait(false);
        }
    }
}