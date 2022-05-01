using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Dtos;
using SocialNetwork.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialNetwork.Profiles
{
    public class EverythingProfile : Profile
    {
        public EverythingProfile()
        {
            // source => target
            CreateMap<Post, PostReadDto>()
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.GetLikesCount()))
                .ForMember(dest => dest.DislikesCount, opt => opt.MapFrom(src => src.GetDislikesCount()))
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.FilesDirs))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<UserCreateDto, User>();
            CreateMap<User, UserReadDto>();
        }
    }
}
