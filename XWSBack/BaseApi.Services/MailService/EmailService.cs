using System;
using System.Threading.Tasks;
using BaseApi.Services.ConfigurationContracts;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BaseApi.Services.MailService
{
    public class EmailService
    {
        private readonly SendGridContract _sendGridContract;
        private readonly ILogger<EmailService> _logger;
        private SendGridClient _client;

        private const string VerificationTemplate = "d-ce89fa0a658b45b9bfd46ed0712e1cc8";
        private const string VerificationLinkTemplate = "https://localhost:44322/api/Register/Confirm?userId={0}&token={1}";

        private const string RecoveryTemplate = "d-da61904271ad475ead15b4663004dabc";

        private const string PasswordlessTemplate = "d-6bca0faa0d8f4425bf996adcdcb9472c";
        private const string PasswordlessLoginTemplate = "https://localhost:44322/api/login/our/passwordless/{0}/{1}";
        
        private const string SuccessfulMail = "Mail sent to mail address : {0}";
        private const string FailedMail = "Couldn't send email to mail address : {0}";

        public EmailService(SendGridContract sendGridContract, ILogger<EmailService> logger)
        {
            _sendGridContract = sendGridContract;
            _logger = logger;
        }
        
        public async Task VerifyEmail(string email, Guid userId, string token)
        {
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

            await SendTemplatedMail(message, email);
        }

        public async Task RecoverAccount(string email, string token)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridContract.From),
                TemplateId = RecoveryTemplate
            };
            message.AddTo(email);
            message.SetTemplateData(new
            {
                token
            });

            await SendTemplatedMail(message, email);
        }

        public async Task SendPasswordlessToken(string email, string token, Guid id)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridContract.From),
                TemplateId = PasswordlessTemplate
            };
            message.AddTo(email);
            message.SetTemplateData(new
            {
                loginUrl = string.Format(PasswordlessLoginTemplate, token, id)
            });

            await SendTemplatedMail(message, email);
        }
        
        private async Task SendTemplatedMail(SendGridMessage message, string email)
        {
            _client = new SendGridClient(_sendGridContract.Secret);

            var response = await _client.SendEmailAsync(message);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(string.Format(SuccessfulMail, email));
            }
            else
            {
                var formatted = string.Format(FailedMail, email);
                _logger.LogInformation(formatted);
                _logger.LogInformation(await response.Body.ReadAsStringAsync());
                throw new Exception(formatted);
            }
        }
    }
}