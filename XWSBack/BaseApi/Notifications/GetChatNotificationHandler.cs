using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using BaseApi.Hubs;
using BaseApi.Messages.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;

namespace BaseApi.Notifications
{
    public class GetChatNotificationHandler : IHandleMessages<GetChatNotification>
    {
        private readonly ILogger<GetChatNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private readonly IMemoryCache _memoryCache;
        
        public GetChatNotificationHandler(ILogger<GetChatNotificationHandler> logger, IHubContext<BaseHub> hub, MemoryCacheEntryOptions memoryCacheEntryOptions, IMemoryCache memoryCache)
        {
            _logger = logger;
            _hub = hub;
            _memoryCacheEntryOptions = memoryCacheEntryOptions;
            _memoryCache = memoryCache;
        }

        private ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;
        
        public async Task Handle(GetChatNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved chat for user {UserId} with user {OtherUserId}", message.UserId, message.OtherUserId);

            if (_dictionary.TryGetValue(message.UserId, out var sender))
            {
                await _hub.Clients.Clients(sender.ConnectionIds).SendAsync("chat", message.OtherUserId, message.Messages);
            }

            var response = new BaseNotification()
            {
                JsonResponse = JsonConvert.SerializeObject(message, Formatting.Indented),
                HttpStatusCode = HttpStatusCode.OK
            };
            _memoryCache.Set(message.CorrelationId, response, _memoryCacheEntryOptions);
        }
    }
}