using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class CreateFollowLinkHandler : IHandleMessages<CreateFollowLinkRequest>

    {
        private readonly UserNodeService _userNodeService;
        private readonly FollowLinkService _followLinkService;

        public CreateFollowLinkHandler(UserNodeService userNodeService, FollowLinkService followLinkService)
        {
            _userNodeService = userNodeService;
            _followLinkService = followLinkService;
        }
        
        public async Task Handle(CreateFollowLinkRequest message, IMessageHandlerContext context)
        {
            var validationMessage = await Validate(message);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                await context.Reply(new CreateFollowLinkResponse()
                {
                    MessageToLog = validationMessage,
                    CorrelationId = message.CorrelationId
                }).ConfigureAwait(false);
                return;
            }

            if (message.IsReceiverPrivate)
            {
                await _followLinkService.CreateLinkBetweenUsers(message.Sender, message.Receiver, "followRequest", new FollowRequest()
                {
                    DateCreated = DateTime.Now,
                    FollowRequestId = Guid.NewGuid()
                });
            }
            else
            {
                await _followLinkService.CreateLinkBetweenUsers(message.Sender, message.Receiver, "follows", new FollowLink()
                {
                    DateCreated = DateTime.Now,
                    FollowId = Guid.NewGuid()
                });
            }

            await context.Reply(new CreateFollowLinkResponse()
            {
                IsSuccessful = true,
                MessageToLog = message.IsReceiverPrivate ? "Requested" : "Followed",
                CorrelationId = message.CorrelationId
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(CreateFollowLinkRequest message)
        {
            var retVal = string.Empty;

            if (message.Receiver == Guid.Empty)
            {
                retVal += $"Receiver is mandatory!\n";
            }
            else if (!await _userNodeService.UserExists(message.Receiver))
            {
                retVal += $"UserNode with id {message.Receiver} not found\n";
            }

            if (message.Sender == Guid.Empty)
            {
                retVal += $"Sender is mandatory!\n";
            }
            else if (!await _userNodeService.UserExists(message.Sender))
            {
                retVal += $"UserNode with id {message.Sender} not found\n";
            }

            if (await _followLinkService.HasLink<FollowRequest>(message.Sender, message.Receiver, "followRequest"))
            {
                retVal += $"User with id {message.Sender} already submited the follow request to {message.Receiver}\n";
            }
            else if (await _followLinkService.HasLink<FollowLink>(message.Sender, message.Receiver, "follows"))
            {
                retVal += $"User with id {message.Sender} already follows {message.Receiver}\n";
            }
            else if (await _followLinkService.HasLink<BlockLink>(message.Sender, message.Receiver, "blocks"))
            {
                retVal += $"User with id {message.Sender} is blocking {message.Receiver}\n";
            }
            else if (await _followLinkService.HasLink<BlockLink>(message.Receiver, message.Sender, "blocks"))
            {
                retVal += $"User with id {message.Receiver} is blocking {message.Receiver}\n";
            }
            
            return retVal;
        }
    }
}