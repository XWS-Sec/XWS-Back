using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NServiceBus;
using Posts.Messages;
using Posts.Messages.Dtos;
using Posts.Model;

namespace Posts.Handlers.Handlers
{
    public class CommentOnPostHandler : IHandleMessages<CommentRequest>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _postsCollection;

        public CommentOnPostHandler(IMapper mapper, IMongoClient client)
        {
            _mapper = mapper;
            _postsCollection = client.GetDatabase("Posts").GetCollection<Post>("Posts");
        }

        public async Task Handle(CommentRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new CommentResponse()
                {
                    CorrelationId = message.CorrelationId
                }).ConfigureAwait(false);
                return;
            }

            var postCursor = await _postsCollection.FindAsync(x => x.Id == message.PostId);
            var post = postCursor.First();

            if (post.Comments == null)
            {
                post.Comments = new List<Comment>();
            }

            var comment = new Comment()
            {
                Text = message.Text,
                CommenterId = message.UserId,
                DateCreated = DateTime.Now
            };
            post.Comments.Add(comment);

            await _postsCollection.FindOneAndReplaceAsync(x => x.Id == message.PostId, post);

            await context.Reply(new CommentResponse()
            {
                CorrelationId = message.CorrelationId,
                CreatedComment = _mapper.Map<CommentDto>(comment),
                IsSuccessful = true
            }).ConfigureAwait(false);
        }

        public async Task<string> ValidateMessage(CommentRequest request)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(request.Text))
            {
                retVal += "Text cannot be empty\n";
            }

            if (request.UserId == Guid.Empty)
            {
                retVal += "User is mandatory\n";
            }
            
            if (request.PostId == Guid.Empty)
            {
                retVal += "Post is mandatory\n";
            }
            else
            {
                var postCursor = await _postsCollection.FindAsync(x => x.Id == request.PostId);
                if (postCursor.FirstOrDefault() == null)
                {
                    retVal += "Post not found\n";
                }
                
            }
            
            return retVal;
        }
    }
}