using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class UnblockHandler : IHandleMessages<UnblockRequest>
    {
        private readonly UserNodeService _userNodeService;
        private readonly FollowLinkService _followLinkService;

        public UnblockHandler(FollowLinkService followLinkService, UserNodeService userNodeService)
        {
            _followLinkService = followLinkService;
            _userNodeService = userNodeService;
        }

        public async Task Handle(UnblockRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new UnblockResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await _followLinkService.DeleteLinkBetweenUsers(message.RequesterId, message.BlockedUserId, "blocks");

            await context.Reply(new UnblockResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successfully unblocked user"
            }).ConfigureAwait(false);
        }

        private async Task<string> ValidateMessage(UnblockRequest message)
        {
            var retVal = string.Empty;

            if (!await _userNodeService.UserExists(message.RequesterId))
            {
                retVal += "Requester not found\n";
            }

            if (!await _userNodeService.UserExists(message.BlockedUserId))
            {
                retVal += "User to unblock not found\n";
            }

            if (!await _followLinkService.HasLink<BlockLink>(message.RequesterId, message.BlockedUserId, "blocks"))
            {
                retVal += "User does not block that user\n";
            }

            if (message.BlockedUserId == message.RequesterId)
            {
                retVal += "User cannot unblock himself";
            }
            
            return retVal;
        }
    }
}