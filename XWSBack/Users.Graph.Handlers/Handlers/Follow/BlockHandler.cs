using System;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class BlockHandler : IHandleMessages<BlockRequest>
    {
        private readonly FollowLinkService _followLinkService;
        private readonly UserNodeService _userNodeService;

        public BlockHandler(FollowLinkService followLinkService, UserNodeService userNodeService)
        {
            _followLinkService = followLinkService;
            _userNodeService = userNodeService;
        }

        public async Task Handle(BlockRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new BlockResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await _followLinkService.DeleteLinkBetweenUsers(message.RequesterId, message.UserToBlockId, "follows");
            await _followLinkService.DeleteLinkBetweenUsers(message.RequesterId, message.UserToBlockId, "followRequest");
            await _followLinkService.DeleteLinkBetweenUsers(message.UserToBlockId, message.RequesterId, "follows");
            await _followLinkService.DeleteLinkBetweenUsers(message.UserToBlockId, message.RequesterId, "followRequest");

            await _followLinkService.CreateLinkBetweenUsers(message.RequesterId, message.UserToBlockId, "blocks", new BlockLink()
            {
                BlockId = Guid.NewGuid(),
                DateCreated = DateTime.Now
            });

            await context.Reply(new BlockResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successfully blocked"
            }).ConfigureAwait(false);
        }

        private async Task<string> ValidateMessage(BlockRequest request)
        {
            var retVal = string.Empty;

            if (!await _userNodeService.UserExists(request.RequesterId))
            {
                retVal += "Requester not found\n";
            }

            if (!await _userNodeService.UserExists(request.UserToBlockId))
            {
                retVal += "User to block not found\n";
            }

            if (await _followLinkService.HasLink<BlockLink>(request.RequesterId, request.UserToBlockId, "blocks"))
            {
                retVal += "User already blocks that user\n";
            }

            if (request.RequesterId == request.UserToBlockId)
            {
                retVal += "User cannot block the himself";
            }

            return retVal;
        }
    }
}