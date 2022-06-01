using AutoMapper;
using BaseApi.Dto;
using BaseApi.Dto.Milestone;
using BaseApi.Dto.Users;
using BaseApi.Messages.Dtos;
using BaseApi.Model.Mongo;
using Chats.Messages.Dtos;
using JobOffers.Messages.Dtos;
using Posts.Messages.Dtos;

namespace BaseApi.MappingProfiles
{
    public class BaseApiProfile : Profile
    {
        public BaseApiProfile()
        {
            CreateMap<RegisterUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<CommentDto, CommentNotificationDto>();
            CreateMap<PostDto, PostNotificationDto>();
            CreateMap<MessageDto, MessageNotificationDto>();
            CreateMap<CreateMilestoneDto, Milestone>();
            CreateMap<JobOfferDto, JobOfferNotificationDto>();
        }
    }
}