using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JobOffers.Handlers.Services;
using JobOffers.Messages;
using JobOffers.Model;
using MongoDB.Driver;
using NServiceBus;

namespace JobOffers.Handlers.Handlers
{
    public class PublishNewJobOfferHandler : IHandleMessages<PublishNewJobOfferRequest>
    {
        private readonly IMongoCollection<Company> _companyCollection;
        private readonly ApiKeyGenerator _apiKeyGenerator;

        public PublishNewJobOfferHandler(ApiKeyGenerator apiKeyGenerator, IMongoClient client)
        {
            _apiKeyGenerator = apiKeyGenerator;
            _companyCollection = client.GetDatabase("JobOffers").GetCollection<Company>("JobOffers");
        }

        public async Task Handle(PublishNewJobOfferRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await Validate(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new PublishNewJobOfferResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var company = await FindCompanyByApiKey(message.ApiKey);
            company.JobOffers ??= new List<JobOffer>();
            
            company.JobOffers.Add(new JobOffer()
            {
                Description = message.Description,
                Id = Guid.NewGuid(),
                Prerequisites = message.Prerequisites,
                JobTitle = message.JobTitle,
                LinkToJobOffer = message.LinkToJobOffer
            });
            await _companyCollection.ReplaceOneAsync(x => x.Id == company.Id, company);

            await context.Reply(new PublishNewJobOfferResponse()
            {
                IsSuccessful = true,
                CorrelationId = message.CorrelationId,
                MessageToLog = "success"
            }).ConfigureAwait(false);
        }

        private async Task<string> Validate(PublishNewJobOfferRequest message)
        {
            var retVal = string.Empty;

            if (string.IsNullOrEmpty(message.Description))
                retVal += "Description is mandatory\n";

            if (string.IsNullOrEmpty(message.Prerequisites))
                retVal += "Prerequisites are mandatory\n";

            if (string.IsNullOrEmpty(message.JobTitle))
                retVal += "Job title is mandatory\n";

            if (string.IsNullOrEmpty(message.LinkToJobOffer))
                retVal += "Link to job offer has to be provided\n";

            if (string.IsNullOrEmpty(message.ApiKey))
            {
                retVal += "Api key is mandatory";
            }
            else
            {
                var correspondingCompany = await FindCompanyByApiKey(message.ApiKey);

                if (correspondingCompany == null)
                {
                    retVal += "Invalid apikey\n";
                }
            }
            
            return retVal;
        }

        private async Task<Company> FindCompanyByApiKey(string apiKey)
        {
            var hashed = _apiKeyGenerator.ComputeHash(apiKey);
            var correspondingCompanyCursor = await _companyCollection.FindAsync(x => x.ApiKeyHash == hashed);
            var correspondingCompany = await correspondingCompanyCursor.FirstOrDefaultAsync();

            return correspondingCompany;
        }
    }
}