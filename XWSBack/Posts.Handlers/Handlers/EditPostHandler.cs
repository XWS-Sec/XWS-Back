using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Model;

namespace Posts.Handlers.Handlers
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
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new EditPostResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var posts = await _postCollection.FindAsync(x => x.PosterId == message.UserId && x.Id == message.PostId);
            var post = await posts.FirstAsync();

            var shouldDeleteOldPic = message.RemoveOldPic;

            post.Text = message.Text;
            if (message.RemoveOldPic)
            {
                post.HasPicture = message.HasPicture;
            }
            else
            {
                post.HasPicture = post.HasPicture || message.HasPicture;
            }

            await _postCollection.ReplaceOneAsync(x => x.PosterId == message.UserId && x.Id == message.PostId, post);

            await context.Reply(new EditPostResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successful",
                ShouldDeleteOldPicture = shouldDeleteOldPic
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(EditPostRequest message)
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
                var posts = await _postCollection.FindAsync(x => x.Id == message.PostId);
                var post = posts.FirstOrDefault();
                if (post != null)
                {
                    if (post.PosterId != message.UserId)
                    {
                        retVal += $"Post with id {message.PostId} is not owned by user {message.UserId}\n";
                    }
                }
                else
                {
                    retVal += $"Post with id {message.PostId} not found\n";
                }
            }
            
            return retVal;
        }
    }
}