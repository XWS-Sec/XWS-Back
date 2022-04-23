using AutoMapper;
using BaseApi.Dto;
using BaseApi.Messages.Dtos;
using BaseApi.Model.Mongo;
using Posts.Messages.Dtos;

namespace BaseApi.MappingProfiles
{
    public class BaseApiProfile : Profile
    {
        public BaseApiProfile()
        {
            CreateMap<RegisterUserDto, User>();

            CreateMap<CommentDto, CommentNotificationDto>();
            CreateMap<PostDto, PostNotificationDto>();
        }
    }
}