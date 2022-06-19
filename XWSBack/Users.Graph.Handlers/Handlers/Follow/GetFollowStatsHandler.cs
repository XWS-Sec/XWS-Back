using System;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class GetFollowStatsHandler : IHandleMessages<GetFollowStatsRequest>
    {
        private readonly GetFollowStatsService _getFollowStatsService;
        private readonly UserNodeService _userNodeService;

        public GetFollowStatsHandler(GetFollowStatsService getFollowStatsService, UserNodeService userNodeService)
        {
            _getFollowStatsService = getFollowStatsService;
            _userNodeService = userNodeService;
        }
        
        public async Task Handle(GetFollowStatsRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new GetFollowStatsResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            await context.Reply(new GetFollowStatsResponse()
            {
                Followers = await _getFollowStatsService.GetIdsToMe(message.UserId, "follows"),
                Following = await _getFollowStatsService.GetIdsFromMe(message.UserId, "follows"),
                FollowRequests = await _getFollowStatsService.GetIdsToMe(message.UserId, "followRequest"),
                FollowRequested = await _getFollowStatsService.GetIdsFromMe(message.UserId, "followRequest"),
                Blocked = await _getFollowStatsService.GetIdsFromMe(message.UserId, "blocks"),
                BlockedFrom = await _getFollowStatsService.GetIdsToMe(message.UserId, "blocks"),
                IsSuccessful = true,
                MessageToLog = $"Success!",
                CorrelationId = message.CorrelationId,
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(GetFollowStatsRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += $"UserId is mandatory!\n";
            }
            else if (!await _userNodeService.UserExists(message.UserId))
            {
                retVal += $"User with id {message.UserId} not found\n";   
            }
            
            return retVal;
        }
    }
}