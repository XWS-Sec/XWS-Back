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
    public class SkillStatsNotificationHandler : IHandleMessages<SkillStatsNotification>
    {
        private readonly ILogger<SkillStatsNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public SkillStatsNotificationHandler(ILogger<SkillStatsNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public async Task Handle(SkillStatsNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved {LinkName} for user {UserId}", message.LinkName, message.UserId);

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds)
                    .SendAsync("skillStatsNotification", message.Skills, message.LinkName);
            }
        }
    }
}