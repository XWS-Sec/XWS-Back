﻿using System;

namespace BaseApi.Services.ConfigurationContracts
{
    public class SendGridContract
    {
        public SendGridContract()
        {
            var envSecret = Environment.GetEnvironmentVariable("XWS_SENDGRID");
            if (string.IsNullOrEmpty(envSecret))
                throw new Exception("Sendgrid api key missing");

            Secret = envSecret;
            From = "milosavljevic.ra5.2018@uns.ac.rs";
        }
        
        public string Secret { get; }
        public string From { get; }
    }
}