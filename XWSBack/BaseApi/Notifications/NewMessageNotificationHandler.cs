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
    public class NewMessageNotificationHandler : IHandleMessages<NewMessageNotification>
    {
        private readonly ILogger<NewMessageNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public NewMessageNotificationHandler(IHubContext<BaseHub> hub, ILogger<NewMessageNotificationHandler> logger)
        {
            _hub = hub;
            _logger = logger;
        }
        
        public async Task Handle(NewMessageNotification message, IMessageHandlerContext context)
        {
            _logger.Log(LogLevel.Information, $"New message from {message.SenderId} to {message.ReceiverId}");

            if (_dictionary.TryGetValue(message.SenderId, out var sender))
            {
                await _hub.Clients.Clients(sender.ConnectionIds).SendAsync("newMessage",
                    new { message.SenderId, message.ReceiverId, message.Message, message.DateCreated });
            }

            if (_dictionary.TryGetValue(message.ReceiverId, out var receiver))
            {
                await _hub.Clients.Clients(receiver.ConnectionIds).SendAsync("newMessage",
                    new { message.SenderId, message.ReceiverId, message.Message, message.DateCreated });
            }
        }
    }
}