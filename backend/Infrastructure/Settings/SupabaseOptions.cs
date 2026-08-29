namespace Infrastructure.Settings;

public sealed class SupabaseOptions {
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "ticket-images";
}
