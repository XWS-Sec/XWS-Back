using System;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.Dto.Company;
using BaseApi.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : SyncController
    {
        private readonly IMessageSession _session;

        public CompanyController(IMessageSession session, IMemoryCache cache) : base(cache)
        {
            _session = session;
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewCompanyDto newCompany)
        {
            var request = new BeginCreateCompanyRequest()
            {
                Email = newCompany.Email,
                Name = newCompany.Name,
                CorrelationId = Guid.NewGuid(),
                PhoneNumber = newCompany.PhoneNumber,
                LinkToCompany = newCompany.LinkToCompany
            };

            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);

            return ReturnBaseNotification(response);
        }
    }
}