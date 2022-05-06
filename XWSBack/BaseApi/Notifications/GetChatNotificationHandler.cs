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
    public class GetChatNotificationHandler : IHandleMessages<GetChatNotification>
    {
        private readonly ILogger<GetChatNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;

        public GetChatNotificationHandler(ILogger<GetChatNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;
        
        public async Task Handle(GetChatNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved chat for user {UserId} with user {OtherUserId}", message.UserId, message.OtherUserId);

            if (_dictionary.TryGetValue(message.UserId, out var sender))
            {
                await _hub.Clients.Clients(sender.ConnectionIds).SendAsync("chat", message.OtherUserId, message.Messages);
            }
        }
    }
}