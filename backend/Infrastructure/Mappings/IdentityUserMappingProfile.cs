using Application.DTOs.Users;
using AutoMapper;

namespace Infrastructure.Mappings;

public sealed class IdentityUserMappingProfile : Profile {
    public IdentityUserMappingProfile() {
        CreateMap<UserMappingSource, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Profile.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile.LastName))
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(src =>
                    (src.Profile.FirstName + " " + src.Profile.LastName).Trim()))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Identity.Email ?? src.Profile.Email))
            .ForMember(
                dest => dest.EmailConfirmed,
                opt => opt.MapFrom(src => src.Identity.EmailConfirmed))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Profile.Phone))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Profile.Address))
            .ForMember(
                dest => dest.AvatarImageId,
                opt => opt.MapFrom(src => src.Profile.AvatarImageId))
            .ForMember(
                dest => dest.AvatarUrl,
                opt => opt.MapFrom(src =>
                    src.Profile.AvatarImage != null ? src.Profile.AvatarImage.Url : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Profile.Status))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Profile.CreatedAt));
    }
}
