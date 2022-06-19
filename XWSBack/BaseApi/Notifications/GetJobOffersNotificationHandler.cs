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
    public class GetJobOffersNotificationHandler : IHandleMessages<GetJobOffersNotification>
    {
        private readonly ILogger<GetJobOffersNotificationHandler> _logger;
        private readonly IHubContext<BaseHub> _hub;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private static ConcurrentDictionary<Guid, ConnectedUser> _dictionary => BaseHub.connections;
        private readonly IMemoryCache _memoryCache;

        public GetJobOffersNotificationHandler(ILogger<GetJobOffersNotificationHandler> logger, IHubContext<BaseHub> hub, MemoryCacheEntryOptions memoryCacheEntryOptions, IMemoryCache memoryCache)
        {
            _logger = logger;
            _hub = hub;
            _memoryCacheEntryOptions = memoryCacheEntryOptions;
            _memoryCache = memoryCache;
        }

        public async Task Handle(GetJobOffersNotification message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Retrieved job offers for users : {UserId}", message.UserId);
            
            if (_dictionary.TryGetValue(message.UserId, out var user))
            {
                await _hub.Clients.Clients(user.ConnectionIds).SendAsync("jobOffersNotification", message.JobOffers);
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