namespace Infrastructure.Settings;

public sealed class SeedOptions {
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@tickets.local";
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFirstName { get; set; } = "System";
    public string AdminLastName { get; set; } = "Admin";
}
