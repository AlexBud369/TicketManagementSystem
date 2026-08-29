using Application.DTOs.Images;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public sealed class ImagesMappingProfile : Profile {
    public ImagesMappingProfile() {
        CreateMap<Image, ImageDto>();
    }
}
