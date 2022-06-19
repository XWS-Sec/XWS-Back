using System;
using System.Net;
using System.Threading;
using BaseApi.Messages.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BaseApi.Controllers.Base
{
    public class SyncController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        private static int RetryInterval = 100;
        private int _maxRetryCounter = 50;

        public SyncController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        protected BaseNotification SyncResponse(Guid correlationId)
        {
            if (!_memoryCache.TryGetValue(correlationId, out BaseNotification response))
            {
                _maxRetryCounter--;
                if (_maxRetryCounter == 0)
                    return new BaseNotification()
                    {
                        JsonResponse = $"no response received",
                        HttpStatusCode = HttpStatusCode.BadRequest
                    };
                
                Thread.Sleep(RetryInterval);
                return SyncResponse(correlationId);
            }
            
            _memoryCache.Remove(correlationId);
            return response;
        }

        protected IActionResult ReturnBaseNotification(BaseNotification notification)
        {
            return notification.HttpStatusCode == HttpStatusCode.OK
                ? Ok(notification.JsonResponse)
                : BadRequest(notification.JsonResponse);
        }
    }
}