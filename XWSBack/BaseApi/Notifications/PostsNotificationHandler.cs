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
    public class PostsNotificationHandler : IHandleMessages<PostsNotification>
    {
        private readonly ILogger<PostsNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;

        public PostsNotificationHandler(ILogger<PostsNotificationHandler> logger, IHubContext<BaseHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;
        
        public async Task Handle(PostsNotification message, IMessageHandlerContext context)
        {
            _logger.Log(LogLevel.Information, $"Retrieved posts for user {message.UserId}");

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds)
                    .SendAsync("postsNotification", message.Posts);
            }
        }
    }
}