using AutoMapper;
using Posts.Messages.Dtos;
using Posts.Model;

namespace Posts.Handlers.MappingProfiles
{
    public class PostsProfile : Profile
    {
        public PostsProfile()
        {
            CreateMap<Comment, CommentDto>();
            CreateMap<Post, PostDto>();
        }
    }
}