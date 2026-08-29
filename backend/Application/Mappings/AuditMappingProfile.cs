using Application.DTOs.Audit;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public sealed class AuditMappingProfile : Profile {
    public AuditMappingProfile() {
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(
                dest => dest.UserEmail,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));
    }
}
