using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

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

        await using var stream = file.OpenReadStream();
        var signature = new byte[12];
        var bytesRead = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
        if (!HasValidSignature(extension, signature, bytesRead))
            throw new InvalidOperationException("The uploaded file does not contain a valid JPG, PNG or WebP image.");

        stream.Position = 0;

        var containerName = configuration["Azure:ContainerName"] ?? "event-banners";
        var container = new BlobContainerClient(connectionString, containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = expectedType }
        }, cancellationToken);

        return blob.Uri.ToString();
    }

    private static bool HasValidSignature(string extension, byte[] bytes, int length)
    {
        if (extension is ".jpg" or ".jpeg")
            return length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

        if (extension == ".png")
            return length >= 8
                && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        if (extension == ".webp")
            return length >= 12
                && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
                && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";

        return false;
    }
}
