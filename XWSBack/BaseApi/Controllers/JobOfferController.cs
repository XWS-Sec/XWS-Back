using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.CustomAttributes;
using BaseApi.Dto.JobOffer;
using BaseApi.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobOfferController : SyncController
    {
        private readonly IMessageSession _session;

        public JobOfferController(IMessageSession session, IMemoryCache cache) : base(cache)
        {
            _session = session;
        }

        [HttpPost]
        [CheckApiKey]
        public async Task<IActionResult> Post(NewJobOfferDto dto)
        {
            var apiKey = HttpContext.Request.Headers["X-API-KEY"].First();
            var request = new BeginPublishNewJobOffer()
            {
                Description = dto.Description,
                Prerequisites = dto.Prerequisites,
                ApiKey = apiKey,
                CorrelationId = Guid.NewGuid(),
                JobTitle = dto.JobTitle,
                LinkToJobOffer = dto.LinkToJobOffer
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
    }
}