using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Identity;

public class ApplicationUser : IdentityUser<Guid> {
    public User? Profile { get; set; }
}
