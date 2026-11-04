using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Web.Services;

public sealed class AzureBlobStorageService(IConfiguration configuration)
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public async Task<string> UploadEventBannerAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new InvalidOperationException("Choose an image smaller than 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        if (!allowedTypes.TryGetValue(extension, out var expectedType)
            || !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only JPG, PNG and WebP banner images are accepted.");

        var connectionString = configuration["Azure:StorageConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set Azure__StorageConnectionString before uploading a banner.");

        var containerName = configuration["Azure:ContainerName"] ?? "event-banners";
        var container = new BlobContainerClient(connectionString, containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = expectedType }
        }, cancellationToken);

        return blob.Uri.ToString();
    }
}
