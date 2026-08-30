using System.Net;
using System.Net.Http.Headers;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage;

public sealed class SupabaseStorageService : IStorageService {
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;

    public SupabaseStorageService(
        HttpClient httpClient,
        IOptions<SupabaseOptions> options) {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default) {
        EnsureConfigured();

        var objectPath = NormalizeObjectPath(fileName);

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            BuildObjectUri(objectPath)) {
            Content = content
        };
        request.Headers.TryAddWithoutValidation("x-upsert", "true");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode) {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw AppException.BusinessRule(
                $"Failed to upload file to storage. Supabase returned {(int)response.StatusCode}: {body}");
        }

        return BuildPublicUrl(objectPath);
    }

    public async Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default) {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(fileUrl)) {
            return;
        }

        var objectPath = GetObjectPathFromUrl(fileUrl);
        if (string.IsNullOrWhiteSpace(objectPath)) {
            return;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            BuildObjectUri(objectPath));

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound) {
            return;
        }

        if (!response.IsSuccessStatusCode) {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw AppException.BusinessRule(
                $"Failed to delete file from storage. Supabase returned {(int)response.StatusCode}: {body}");
        }
    }

    private void EnsureConfigured() {
        if (string.IsNullOrWhiteSpace(_options.Url) ||
            string.IsNullOrWhiteSpace(_options.ServiceRoleKey) ||
            string.IsNullOrWhiteSpace(_options.Bucket)) {
            throw AppException.BusinessRule(
                "Supabase storage is not configured. Set Supabase:Url, Supabase:ServiceRoleKey, and Supabase:Bucket.");
        }
    }

    private Uri BuildObjectUri(string objectPath) {
        var escapedPath = string.Join(
            '/',
            objectPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

        return new Uri(
            $"{TrimBaseUrl()}/storage/v1/object/{Uri.EscapeDataString(_options.Bucket)}/{escapedPath}",
            UriKind.Absolute);
    }

    private string BuildPublicUrl(string objectPath) {
        return $"{TrimBaseUrl()}/storage/v1/object/public/{_options.Bucket}/{objectPath}";
    }

    private string GetObjectPathFromUrl(string fileUrl) {
        var publicPrefix = $"{TrimBaseUrl()}/storage/v1/object/public/{_options.Bucket}/";
        var privatePrefix = $"{TrimBaseUrl()}/storage/v1/object/{_options.Bucket}/";

        if (fileUrl.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase)) {
            return fileUrl[publicPrefix.Length..];
        }

        if (fileUrl.StartsWith(privatePrefix, StringComparison.OrdinalIgnoreCase)) {
            return fileUrl[privatePrefix.Length..];
        }

        return string.Empty;
    }

    private string TrimBaseUrl() {
        return _options.Url.TrimEnd('/');
    }

    private static string NormalizeObjectPath(string fileName) {
        return fileName.Replace('\\', '/').TrimStart('/');
    }
}
