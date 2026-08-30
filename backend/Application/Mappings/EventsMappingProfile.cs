using Application.DTOs.Events;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public sealed class EventsMappingProfile : Profile {
    public EventsMappingProfile() {
        CreateMap<EventEntity, EventDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src =>
                    src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(
                dest => dest.OrganizerName,
                opt => opt.MapFrom(src =>
                    src.Organizer != null
                        ? (src.Organizer.FirstName + " " + src.Organizer.LastName).Trim()
                        : string.Empty))
            .ForMember(
                dest => dest.BannerImageUrl,
                opt => opt.MapFrom(src =>
                    src.BannerImage != null ? src.BannerImage.Url : null))
            .ForMember(
                dest => dest.SoldTickets,
                opt => opt.MapFrom(src => src.MaxCapacity - src.AvailableTickets))
            .ForMember(
                dest => dest.IsSoldOut,
                opt => opt.MapFrom(src => src.AvailableTickets <= 0));

        CreateMap<EventEntity, EventListDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src =>
                    src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(
                dest => dest.BannerImageUrl,
                opt => opt.MapFrom(src =>
                    src.BannerImage != null ? src.BannerImage.Url : null))
            .ForMember(
                dest => dest.IsSoldOut,
                opt => opt.MapFrom(src => src.AvailableTickets <= 0));
    }
}
