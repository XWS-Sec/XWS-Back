using System;
using System.Threading.Tasks;
using Neo4jClient;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class AnswerFollowRequestHandler : IHandleMessages<AnswerFollowRequest>
    {
        private readonly FollowLinkService _followLinkService;
        private readonly UserNodeService _userNodeService;

        public AnswerFollowRequestHandler(FollowLinkService followLinkService, UserNodeService userNodeService)
        {
            _followLinkService = followLinkService;
            _userNodeService = userNodeService;
        }
        
        public async Task Handle(AnswerFollowRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new AnswerFollowResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await _followLinkService.DeleteLinkBetweenUsers(message.FollowerId, message.ObservedId, "followRequest");
            if (message.IsAccepted)
            {
                await _followLinkService.CreateLinkBetweenUsers(message.FollowerId, message.ObservedId, "follows",
                    new FollowLink()
                    {
                        CreatedDate = DateTime.Now,
                        FollowId = Guid.NewGuid()
                    });   
            }

            await context.Reply(new AnswerFollowResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = $"Successful operation!"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(AnswerFollowRequest message)
        {
            var retVal = string.Empty;

            if (message.FollowerId == Guid.Empty)
            {
                retVal += $"FollowerId is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.FollowerId))
            {
                retVal += $"User with id {message.FollowerId} not found\n";
            }

            if (message.ObservedId == Guid.Empty)
            {
                retVal += $"ObservedId is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.ObservedId))
            {
                retVal += $"User with id {message.ObservedId} not found\n";
            }

            if (!string.IsNullOrEmpty(retVal))
                return retVal;
            
            if (await _followLinkService.HasLink<FollowLink>(message.FollowerId, message.ObservedId, "follows"))
            {
                retVal += $"User with id {message.FollowerId} already follows {message.ObservedId}\n";
            }
            else if (!await _followLinkService.HasLink<FollowRequest>(message.FollowerId, message.ObservedId, "followRequest"))
            {
                retVal += $"User with id {message.FollowerId} doesn't have a follow request towards {message.ObservedId}\n";
            }
            
            return retVal;
        }
    }
}