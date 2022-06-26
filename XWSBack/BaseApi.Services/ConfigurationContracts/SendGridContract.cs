using System;
using BaseApi.Services.Exceptions;

namespace BaseApi.Services.ConfigurationContracts
{
    public class SendGridContract
    {
        public SendGridContract()
        {
            var envSecret = Environment.GetEnvironmentVariable("XWS_SENDGRID");
            if (string.IsNullOrEmpty(envSecret))
                throw new ValidationException("Sendgrid api key missing");

            if (envSecret.StartsWith("{"))
                envSecret = envSecret.Substring(1, envSecret.Length - 2);

            Secret = envSecret;
            From = "milosavljevic.ra5.2018@uns.ac.rs";
        }
        
        public string Secret { get; }
        public string From { get; }
    }
}