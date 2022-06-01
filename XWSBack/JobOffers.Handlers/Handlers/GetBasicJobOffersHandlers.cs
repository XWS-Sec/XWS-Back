using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JobOffers.Messages;
using JobOffers.Messages.Dtos;
using JobOffers.Model;
using MongoDB.Driver;
using NServiceBus;

namespace JobOffers.Handlers.Handlers
{
    public class GetBasicJobOffersHandlers : IHandleMessages<GetBasicJobOffersRequest>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Company> _companyCollection;

        public GetBasicJobOffersHandlers(IMongoClient client,IMapper mapper)
        {
            _mapper = mapper;
            _companyCollection = client.GetDatabase("JobOffers").GetCollection<Company>("JobOffers");
        }

        public async Task Handle(GetBasicJobOffersRequest message, IMessageHandlerContext context)
        {
            var companyCursors =
                await _companyCollection.FindAsync(x => x.JobOffers != null && x.JobOffers.Count() != 0);

            var companies = companyCursors.ToEnumerable();
            var jobOffers = companies.SelectMany(x => x.JobOffers).ToList();

            await context.Reply(new GetBasicJobOffersResponse()
            {
                CorrelationId = message.CorrelationId,
                JobOffers = _mapper.Map<List<JobOfferDto>>(jobOffers),
            }).ConfigureAwait(false);
        }
    }
}