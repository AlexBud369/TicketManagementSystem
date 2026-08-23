namespace Application.Interfaces;

public interface IAppSettings {
    string FrontendUrl { get; }
    int MaxImageSizeMb { get; }
    double ServiceFeePercentage { get; }
    string[] AllowedImageExtensions { get; }
}
