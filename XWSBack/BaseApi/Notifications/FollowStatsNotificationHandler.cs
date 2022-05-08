using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BaseApi.Hubs;
using BaseApi.Messages.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace BaseApi.Notifications
{
    public class FollowStatsNotificationHandler : IHandleMessages<FollowStatsNotification>
    {
        private readonly ILogger<FollowStatsNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public FollowStatsNotificationHandler(ILogger<FollowStatsNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public async Task Handle(FollowStatsNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved follow stats for user {UserId}", message.UserId);

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds).SendAsync("followStatsNotification", message.Followers,
                    message.Following, message.FollowRequests);
            }
        }
    }
}