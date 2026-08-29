using Application.Interfaces;

namespace Infrastructure.Settings;

public sealed class AppSettings : IAppSettings {
    public const string SectionName = "AppSettings";

    public string FrontendUrl { get; set; } = "http://localhost:4200";
    public int MaxImageSizeMb { get; set; } = 5;
    public double ServiceFeePercentage { get; set; } = 0.05;
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
