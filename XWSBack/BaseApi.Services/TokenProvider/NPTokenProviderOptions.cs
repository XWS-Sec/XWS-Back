using System;
using Microsoft.AspNetCore.Identity;

namespace BaseApi.Services.TokenProvider
{
    public class NPTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public NPTokenProviderOptions()
        {
            Name = "NPTokenProvider";
            TokenLifespan = TimeSpan.FromMinutes(30);
        }
    }
}