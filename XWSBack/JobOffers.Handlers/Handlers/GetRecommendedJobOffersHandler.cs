using AutoMapper;
using JobOffers.Model;
using MongoDB.Driver;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobOffers.Messages;
using JobOffers.Messages.Dtos;

namespace JobOffers.Handlers.Handlers
{
    public class GetRecommendedJobOffersHandler : IHandleMessages<GetRecommendedJobOffersRequest>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Company> _companyCollection;

        public GetRecommendedJobOffersHandler(IMongoClient client, IMapper mapper)
        {
            _mapper = mapper;
            _companyCollection = client.GetDatabase("JobOffers").GetCollection<Company>("JobOffers");
        }

        public async Task Handle(GetRecommendedJobOffersRequest message, IMessageHandlerContext context)
        {
            var companyCursors =
                await _companyCollection.FindAsync(x => x.JobOffers != null && x.JobOffers.Any());

            var companies = companyCursors.ToEnumerable();
            var jobOffers = companies.SelectMany(x => x.JobOffers).ToList();

            var recommendedJobOffers = GetRecommendedJobOffers(jobOffers, message.Interests);

            await context.Reply(new GetRecommendedJobOffersResponse()
            {
                CorrelationId = message.CorrelationId,
                JobOffers = _mapper.Map<List<JobOfferDto>>(recommendedJobOffers),
            }).ConfigureAwait(false);

        }

        private static IEnumerable<JobOffer> GetRecommendedJobOffers(IEnumerable<JobOffer> jobOffers, IEnumerable<string> interests)
        {
            var recommendedJobOffers = new List<JobOffer>();

            foreach (var jobOffer in jobOffers)
                if (IsInterested(jobOffer, interests))
                    recommendedJobOffers.Add(jobOffer);

            return recommendedJobOffers;
        }

        private static bool IsInterested(JobOffer jobOffer, IEnumerable<string> interests)
        {
            var prerequisites = jobOffer.Prerequisites;

            foreach (var interest in interests)
                if (!string.IsNullOrEmpty(interest) && prerequisites.ToLower().Contains(interest.ToLower()))
                    return true;


            return false;
        }
    }
}
