﻿using AutoMapper;
using BaseApi.Dto;
using BaseApi.Messages.Dtos;
using BaseApi.Model.Mongo;
using Chats.Messages.Dtos;
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

            CreateMap<MessageDto, MessageNotificationDto>();
        }
    }
}