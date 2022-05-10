using System;
using System.Threading.Tasks;
using BaseApi.Services.ConfigurationContracts;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BaseApi.Services.EmailService
{
    public class EmailService
    {
        private readonly SendGridContract _sendGridContract;
        private readonly ILogger<EmailService> _logger;
        private SendGridClient _client;

        private const string VerificationTemplate = "d-ce89fa0a658b45b9bfd46ed0712e1cc8";
        private const string VerificationLinkTemplate = "https://localhost:44322/api/Register/Confirm?userId={0}&token={1}";

        private const string SuccessfulVerificationMail = "Verification email sent to mail address : {0}";
        private const string FailedVerificationMail = "Couldn't send verification email to mail address : {0}";
        
        public EmailService(SendGridContract sendGridContract, ILogger<EmailService> logger)
        {
            _sendGridContract = sendGridContract;
            _logger = logger;
        }
        
        public async Task VerifyEmail(string email, Guid userId, string token)
        {
            _client = new SendGridClient(_sendGridContract.Secret);
            var message = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridContract.From),
                TemplateId = VerificationTemplate
            };
            message.AddTo(email);
            message.SetTemplateData(new
            {
                buttonUrl = string.Format(VerificationLinkTemplate, userId, token)
            });

            var response = await _client.SendEmailAsync(message);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(string.Format(SuccessfulVerificationMail, email));
            }
            else
            {
                var formatted = string.Format(FailedVerificationMail, email);
                _logger.LogInformation(formatted);
                _logger.LogInformation(await response.Body.ReadAsStringAsync());
                throw new Exception(formatted);
            }
        }
    }
}