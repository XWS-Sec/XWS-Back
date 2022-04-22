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
    public class StandardNotificationHandler : IHandleMessages<StandardNotification>
    {
        private readonly ILogger<StandardNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public StandardNotificationHandler(ILogger<StandardNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }
        
        public async Task Handle(StandardNotification message, IMessageHandlerContext context)
        {
            _logger.Log(message.IsSuccessful ? LogLevel.Information : LogLevel.Warning,
                $"[User: {message.UserId}] - {message.Message}");

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds).SendAsync("notification", message.Message);
            }
        }
    }
}