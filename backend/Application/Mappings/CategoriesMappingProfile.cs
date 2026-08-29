using Application.DTOs.Categories;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public sealed class CategoriesMappingProfile : Profile {
    public CategoriesMappingProfile() {
        CreateMap<Category, CategoryDto>()
            .ForMember(
                dest => dest.EventsCount,
                opt => opt.MapFrom(src => src.Events.Count));
    }
}
