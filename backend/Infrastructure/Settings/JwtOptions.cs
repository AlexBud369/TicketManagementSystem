namespace Infrastructure.Settings;

public sealed class JwtOptions {
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TicketManagement";
    public string Audience { get; set; } = "TicketManagement";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public int RememberMeRefreshTokenDays { get; set; } = 30;
}
