using AutoMapper;
using JobOffers.Messages.Dtos;
using JobOffers.Model;

namespace JobOffers.Handlers.MappingProfiles
{
    public class BasicProfile : Profile
    {
        public BasicProfile()
        {
            CreateMap<JobOffer, JobOfferDto>();
        }
    }
}