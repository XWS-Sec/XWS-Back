using AutoMapper;
using BaseApi.Dto;
using BaseApiModel.Mongo;

namespace BaseApi.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterUserDto, User>();
        }
    }
}