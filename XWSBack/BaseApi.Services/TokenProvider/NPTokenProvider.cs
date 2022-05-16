using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaseApi.Services.TokenProvider
{
    public class NPTokenProvider<T> : DataProtectorTokenProvider<T> where T : class
    {
        public NPTokenProvider(IDataProtectionProvider dataProtectionProvider, 
            IOptions<DataProtectionTokenProviderOptions> options, 
            ILogger<DataProtectorTokenProvider<T>> logger) 
            : base(dataProtectionProvider, options, logger)
        {
        }
    }
}