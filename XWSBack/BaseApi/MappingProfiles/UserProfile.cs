using AutoMapper;
using BaseApi.Dto;
using BaseApi.Model.Mongo;

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