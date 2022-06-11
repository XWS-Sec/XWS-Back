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
    public class StandardNotificationHandler : IHandleMessages<StandardNotification>
    {
        private readonly ILogger<StandardNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private static ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;

        public StandardNotificationHandler(ILogger<StandardNotificationHandler> logger, IHubContext<BaseHub> hub, IMemoryCache memoryCache, MemoryCacheEntryOptions memoryCacheEntryOptions)
        {
            _logger = logger;
            _hub = hub;
            _memoryCache = memoryCache;
            _memoryCacheEntryOptions = memoryCacheEntryOptions;
        }
        
        public async Task Handle(StandardNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("[User: {UserId}] - {Message}", message.UserId, message.Message);

            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds).SendAsync("notification", message.Message);
            }
            
            if (message.CorrelationId == Guid.Empty) return;

            var response = new BaseNotification()
            {
                JsonResponse = JsonConvert.SerializeObject(message, Formatting.Indented),
                HttpStatusCode = message.IsSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest
            };
            _memoryCache.Set(message.CorrelationId, response, _memoryCacheEntryOptions);
        }
    }
}