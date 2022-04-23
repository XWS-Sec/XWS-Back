using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Messages.Dtos;
using Posts.Model;

namespace Posts.Handlers.Handlers
{
    public class GetPostsHandler : IHandleMessages<GetPostsRequest>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _postsCollection;
        private static int pageSize = 10;

        public GetPostsHandler(IMapper mapper, IMongoClient client)
        {
            _mapper = mapper;
            _postsCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }


        public async Task Handle(GetPostsRequest message, IMessageHandlerContext context)
        {
            var response = new GetPostsResponse()
            {
                CorrelationId = message.CorrelationId,
            };
            
            if (message.PostsOwners.Any())
            {
                var filter = Builders<Post>.Filter.In(x => x.PosterId, message.PostsOwners);

                var posts = await _postsCollection.FindAsync(filter, new FindOptions<Post>()
                {
                    Sort = Builders<Post>.Sort.Descending(x => x.DateCreated),
                    Limit = pageSize,
                    Skip = message.Page >= 0 ? message.Page * pageSize : 0,
                });
                
                var foundPosts = posts.ToList();
                
                if (foundPosts != null)
                {
                    response.Posts = _mapper.Map<List<PostDto>>(foundPosts);
                }
            }

            await context.Reply(response).ConfigureAwait(false);
        }
    }
}