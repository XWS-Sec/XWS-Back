using System;
using System.Threading.Tasks;
using JobOffers.Handlers.Services;
using JobOffers.Messages;
using JobOffers.Model;
using MongoDB.Driver;
using NServiceBus;

namespace JobOffers.Handlers.Handlers
{
    public class CreateCompanyRequestHandler : IHandleMessages<CreateCompanyRequest>
    {
        private readonly IMongoCollection<Company> _companyCollection;
        private readonly ApiKeyGenerator _apiKeyGenerator;

        public CreateCompanyRequestHandler(IMongoClient client, ApiKeyGenerator apiKeyGenerator)
        {
            _apiKeyGenerator = apiKeyGenerator;
            _companyCollection = client.GetDatabase("JobOffers").GetCollection<Company>("JobOffers");
        }

        public async Task Handle(CreateCompanyRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);
            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new CreateCompanyResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var generatedKey = await _apiKeyGenerator.GetApiKey();
            await _companyCollection.InsertOneAsync(new Company()
            {
                Email = message.Email,
                Id = Guid.NewGuid(),
                Name = message.Name,
                PhoneNumber = message.PhoneNumber,
                ApiKeyHash = _apiKeyGenerator.ComputeHash(generatedKey),
                LinkToCompany = message.LinkToCompany
            });

            await context.Reply(new CreateCompanyResponse()
            {
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                GeneratedApiKey = generatedKey,
                MessageToLog = "Success"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(CreateCompanyRequest message)
        {
            string retVal = string.Empty;

            if (string.IsNullOrEmpty(message.Email))
            {
                retVal += "Email is mandatory\n";
            }
            else
            {
                var foundCompanyCursor = await _companyCollection.FindAsync(x => x.Email == message.Email);
                var foundCompany = await foundCompanyCursor.FirstOrDefaultAsync();
                if (foundCompany != null)
                    retVal += "Company with that email is already registered\n";
            }

            if (string.IsNullOrEmpty(message.Name))
            {
                retVal += "Name is mandatory\n";
            }

            if (string.IsNullOrEmpty(message.PhoneNumber))
            {
                retVal += "Phone number is mandatory\n";
            }

            if (string.IsNullOrEmpty(message.LinkToCompany))
            {
                retVal += "Link to company is required";
            }

            return retVal;
        }
    }
}