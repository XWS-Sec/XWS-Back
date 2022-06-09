using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Model;

namespace Posts.Handlers.Handlers
{
    public class LikeDislikeHandler : IHandleMessages<LikeDislikeRequest>
    {
        private readonly IMongoCollection<Post> _postsCollection;

        public LikeDislikeHandler(IMongoClient client)
        {
            _postsCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }
        
        public async Task Handle(LikeDislikeRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new LikeDislikeResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                
                return;
            }
            
            var postCursor = await _postsCollection.FindAsync(x => x.Id == message.PostId);
            var post = postCursor.First();

            var messageToLog = string.Empty;
            
            post.Liked ??= new List<Guid>();
            post.Disliked ??= new List<Guid>();
            
            if (message.IsLike)
            {
                if (post.Liked.Contains(message.UserId))
                {
                    messageToLog = "User already liked the post\n";
                }
                else
                {
                    post.Liked.Add(message.UserId);
                    post.Disliked.Remove(message.UserId);
                    await _postsCollection.FindOneAndReplaceAsync(x => x.Id == message.PostId, post);
                }
            }
            else
            {
                if (post.Disliked.Contains(message.UserId))
                {
                    messageToLog = "User already disliked the post\n";
                }
                else
                {
                    post.Disliked.Add(message.UserId);
                    post.Liked.Remove(message.UserId);
                    await _postsCollection.FindOneAndReplaceAsync(x => x.Id == message.PostId, post);
                }
            }

            await context.Reply(new LikeDislikeResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = string.IsNullOrEmpty(messageToLog),
                MessageToLog = string.IsNullOrEmpty(messageToLog)
                    ? "Success"
                    : messageToLog
            }).ConfigureAwait(false);
        }

        private async Task<string> ValidateMessage(LikeDislikeRequest message)
        {
            var retVal = string.Empty;
            if (message.PostId == Guid.Empty)
            {
                retVal += "Post is mandatory\n";
            }
            else
            {
                var postCursor = await _postsCollection.FindAsync(x => x.Id == message.PostId);
                if (postCursor.FirstOrDefault() == null)
                {
                    retVal += "Post not found\n";
                }
            }

            if (message.UserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }

            return retVal;
        }
    }
}