using AutoMapper;
using Chats.Messages.Dtos;
using Chats.Model;

namespace Chats.Handlers.MappingProfiles
{
    public class ChatsProfile : Profile
    {
        public ChatsProfile()
        {
            CreateMap<Message, MessageDto>();
        }
    }
}