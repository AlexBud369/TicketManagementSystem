using Application.DTOs.Orders;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Scheduling;

namespace Application.Mappings;

public sealed class OrdersMappingProfile : Profile {
    public OrdersMappingProfile() {
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(
                dest => dest.TicketNumber,
                opt => opt.MapFrom(src =>
                    src.Ticket != null ? src.Ticket.TicketNumber : string.Empty))
            .ForMember(
                dest => dest.SeatInfo,
                opt => opt.MapFrom(src => src.Ticket != null ? src.Ticket.SeatInfo : null));

        CreateMap<Order, OrderDto>()
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
                dest => dest.EventTitle,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Title : string.Empty))
            .ForMember(
                dest => dest.EventDate,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Date : default))
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
                dest => dest.CanBeCancelled,
                opt => opt.MapFrom(src =>
                    (src.Status == OrderStatus.Pending || src.Status == OrderStatus.Paid) &&
                    (src.EventEntity == null ||
                        !EventSchedule.HasStarted(
                            src.EventEntity.Date,
                            src.EventEntity.StartTime,
                            DateTime.UtcNow))))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<Order, OrderListDto>()
            .ForMember(
                dest => dest.EventTitle,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Title : string.Empty))
            .ForMember(
                dest => dest.EventDate,
                opt => opt.MapFrom(src =>
                    src.EventEntity != null ? src.EventEntity.Date : default))
            .ForMember(
                dest => dest.BuyerName,
                opt => opt.MapFrom(src =>
                    src.User != null
                        ? (src.User.FirstName + " " + src.User.LastName).Trim()
                        : null));

        CreateMap<Order, EventBuyerDto>()
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : string.Empty))
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(src =>
                    src.User != null
                        ? (src.User.FirstName + " " + src.User.LastName).Trim()
                        : string.Empty));
    }
}
