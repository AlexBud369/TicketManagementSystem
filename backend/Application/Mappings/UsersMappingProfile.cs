using Application.DTOs.Users;
using AutoMapper;

namespace Application.Mappings;

public sealed class UsersMappingProfile : Profile {
    public UsersMappingProfile() {
        CreateMap<UserListDtoSource, UserListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(src =>
                    (src.User.FirstName + " " + src.User.LastName).Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(
                dest => dest.RegistrationDate,
                opt => opt.MapFrom(src => src.User.CreatedAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.Status))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
            .ForMember(dest => dest.OrdersCount, opt => opt.MapFrom(src => src.OrdersCount));
    }
}
