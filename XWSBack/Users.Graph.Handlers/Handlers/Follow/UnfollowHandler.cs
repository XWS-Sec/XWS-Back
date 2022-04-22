using System;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class UnfollowHandler : IHandleMessages<UnfollowRequest>
    {
        private readonly UserNodeService _userNodeService;
        private readonly FollowLinkService _followLinkService;

        public UnfollowHandler(UserNodeService userNodeService, FollowLinkService followLinkService)
        {
            _userNodeService = userNodeService;
            _followLinkService = followLinkService;
        }
        
        public async Task Handle(UnfollowRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new UnfollowResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await _followLinkService.DeleteLinkBetweenUsers(message.Sender, message.Receiver, "follows");

            await context.Reply(new UnfollowResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = $"User {message.Sender} successfully unfollowed user {message.Receiver}"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(UnfollowRequest message)
        {
            var retVal = string.Empty;

            if (message.Receiver == Guid.Empty)
            {
                retVal += $"Follower is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.Receiver))
            {
                retVal += $"User with id {message.Receiver} not found\n";
            }

            if (message.Sender == Guid.Empty)
            {
                retVal += $"Sender is mandatory\n";
            }
            else if (!await _userNodeService.UserExists(message.Sender))
            {
                retVal += $"User with id {message.Sender} not found\n";
            }

            if (!string.IsNullOrEmpty(retVal)) return retVal;

            if (!await _followLinkService.HasLink<FollowLink>(message.Sender, message.Receiver, "follows"))
            {
                retVal += $"Follower {message.Receiver} doesn't follow user {message.Sender}\n";
            }
            
            return retVal;
        }
    }
}