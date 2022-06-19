using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using BaseApi.Hubs;
using BaseApi.Messages.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using NServiceBus;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace BaseApi.Notifications
{
    public class FollowStatsNotificationHandler : IHandleMessages<FollowStatsNotification>
    {
        private readonly ILogger<FollowStatsNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private static ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;
        private readonly IMemoryCache _memoryCache;

        public FollowStatsNotificationHandler(ILogger<FollowStatsNotificationHandler> logger, IHubContext<BaseHub> hub, IMemoryCache memoryCache, MemoryCacheEntryOptions memoryCacheEntryOptions)
        {
            _logger = logger;
            _hub = hub;
            _memoryCache = memoryCache;
            _memoryCacheEntryOptions = memoryCacheEntryOptions;
        }

        public async Task Handle(FollowStatsNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved follow stats for user {UserId}", message.UserId);

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds).SendAsync("followStatsNotification", message.Followers,
                    message.Following, message.FollowRequests);
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