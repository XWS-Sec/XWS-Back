using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using PostMessages;
using PostServiceModel;

namespace PostApiEndpoint.Handlers
{
    public class EditPostHandler : IHandleMessages<EditPostRequest>
    {
        private readonly IMongoCollection<Post> _postCollection;

        public EditPostHandler(IMongoClient client)
        {
            _postCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }
        
        public async Task Handle(EditPostRequest message, IMessageHandlerContext context)
        {
            var matchedPosts =
                await _postCollection.FindAsync(x => x.PosterId == message.UserId && x.Id == message.PostId);

            var post = matchedPosts.FirstOrDefault();

            var response = new EditPostResponse()
            {
                PostId = message.PostId,
                TempPicId = message.TempPicId,
            };
            
            if (post != null)
            {
                response.IsSuccessful = true;
                response.ShouldChangePic = true;

                post.Text = message.Text;
                post.HasPicture = message.HasPicture;

                await _postCollection.FindOneAndReplaceAsync(x => x.PosterId == message.UserId &&
                                                                  x.Id == message.PostId, post);
            }

            await context.Reply(response).ConfigureAwait(false);
        }
    }
}