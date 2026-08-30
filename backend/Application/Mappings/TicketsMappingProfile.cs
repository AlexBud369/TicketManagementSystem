using Application.DTOs.Tickets;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings;

public sealed class TicketsMappingProfile : Profile {
    public TicketsMappingProfile() {
        CreateMap<Ticket, TicketDto>()
            .ForMember(
                dest => dest.OrderNumber,
                opt => opt.MapFrom(src =>
                    src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(
                dest => dest.EventTitle,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Title : string.Empty))
            .ForMember(
                dest => dest.EventDate,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Date : default))
            .ForMember(
                dest => dest.EventStartTime,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.StartTime : default))
            .ForMember(
                dest => dest.EventLocation,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Location : string.Empty))
            .ForMember(
                dest => dest.EventBannerUrl,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null && src.EventEntity.BannerImage != null
                        ? src.EventEntity.BannerImage.Url
                        : null))
            .ForMember(
                dest => dest.BuyerName,
                opt => opt.MapFrom(src =>
                    src.User != null
                        ? (src.User.FirstName + " " + src.User.LastName).Trim()
                        : string.Empty))
            .ForMember(
                dest => dest.BuyerEmail,
                opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : string.Empty))
            .ForMember(
                dest => dest.Price,
                opt => opt.MapFrom(src => ResolvePrice(src)))
            .ForMember(
                dest => dest.IsValid,
                opt => opt.MapFrom(src => src.Status == TicketStatus.Active));
    }

    private static decimal ResolvePrice(Ticket ticket) {
        var linePrice = ticket.Order?.OrderItems
            .FirstOrDefault(item => item.TicketId == ticket.Id)
            ?.UnitPrice;

        return linePrice
            ?? ticket.EventEntity?.TicketPrice
            ?? 0m;
    }
}
