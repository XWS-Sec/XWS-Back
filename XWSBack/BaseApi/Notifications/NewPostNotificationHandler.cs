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
    public class NewPostNotificationHandler : IHandleMessages<NewPostNotification>
    {
        private readonly ILogger<NewPostNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;

        public NewPostNotificationHandler(ILogger<NewPostNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        private static ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public async Task Handle(NewPostNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Notifying users of the new post from user {Poster}", message.Poster);

            foreach (var user in message.UsersToNotify)
            {
                if (_dictionary.TryGetValue(user, out var userConnection))
                {
                    await _hub.Clients.Clients(userConnection.ConnectionIds).SendAsync("newPost", message.Poster);
                }
            }
        }
    }
}