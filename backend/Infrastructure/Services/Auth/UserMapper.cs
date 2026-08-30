using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Mappings;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth;

public sealed class UserMapper {
    private readonly IMapper _mapper;

    public UserMapper(IMapper mapper) {
        _mapper = mapper;
    }

    public async Task<UserDto> ToDtoAsync(
        User profile,
        ApplicationUser identityUser,
        UserManager<ApplicationUser> userManager) {
        var roles = await userManager.GetRolesAsync(identityUser);

        return _mapper.Map<UserDto>(new UserMappingSource {
            Profile = profile,
            Identity = identityUser,
            Roles = roles.ToArray()
        });
    }
}
