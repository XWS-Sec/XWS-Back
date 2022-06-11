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
    public class SkillStatsNotificationHandler : IHandleMessages<SkillStatsNotification>
    {
        private readonly ILogger<SkillStatsNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private readonly IMemoryCache _memoryCache;
        private static ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public SkillStatsNotificationHandler(ILogger<SkillStatsNotificationHandler> logger, IHubContext<BaseHub> hub, MemoryCacheEntryOptions memoryCacheEntryOptions, IMemoryCache memoryCache)
        {
            _logger = logger;
            _hub = hub;
            _memoryCacheEntryOptions = memoryCacheEntryOptions;
            _memoryCache = memoryCache;
        }

        public async Task Handle(SkillStatsNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved {LinkName} for user {UserId}", message.LinkName, message.UserId);

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds)
                    .SendAsync("skillStatsNotification", message.Skills, message.LinkName);
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