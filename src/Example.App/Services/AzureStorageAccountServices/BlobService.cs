using System.Net;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Example.App.Configurations;
using Example.App.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Example.App.Services.AzureStorageAccountServices;

public class BlobService
{
    public const int DEFAULTS_EXPIRES_MINUTES = 5;

    private readonly AzureStorageAccountConfiguration azureStorageAccountConfiguration;
    private readonly BlobServiceClient blobServiceClient;
    private readonly ILogger logger;

    public BlobService(IOptionsMonitor<AzureStorageAccountConfiguration> storageConfig, ILogger<BlobService> logger)
    {
        azureStorageAccountConfiguration = storageConfig.CurrentValue ?? throw new ArgumentNullException(nameof(storageConfig));
        this.logger = logger;

        if (string.IsNullOrWhiteSpace(azureStorageAccountConfiguration.ConnectionString))
        {
            StorageSharedKeyCredential storageSharedKeyCredential = new(azureStorageAccountConfiguration.AccountName, azureStorageAccountConfiguration.AccountKey);
            var serviceUri = $"https://{azureStorageAccountConfiguration.AccountName}.blob.core.windows.net";

            blobServiceClient = new(new Uri(serviceUri), storageSharedKeyCredential);
        }
        else
        {
            blobServiceClient = new(azureStorageAccountConfiguration.ConnectionString);
        }

        if (blobServiceClient == null)
        {
            throw new Exception("Invalid azure storage account configuration");
        }
    }

    public Task<string> GenerateReadAccessUrlAsync(string uri, int expiresInMinutes = DEFAULTS_EXPIRES_MINUTES, CancellationToken cancellationToken = default)
        => GenerateAccessUrlAsync(
            uri,
            DateTime.UtcNow.AddMinutes(expiresInMinutes),
            canRead: true,
            cancellationToken: cancellationToken);

    private async Task<string> GenerateAccessUrlAsync(string uri, DateTime? expiresOn = null, bool? canRead = false, bool? canWrite = false, bool? canDelete = false, bool? canDeletePermanently = false, CancellationToken cancellationToken = default)
    {
        string message;

        if (new[] { canRead, canWrite, canDelete, canDeletePermanently }.All(x => x == null))
        {
            message = "At least one permission must be specified";
            throw new ApiException(HttpStatusCode.BadRequest, message);
        }

        DateTimeOffset expiresOnActual = new(expiresOn ?? DateTime.UtcNow.AddMinutes(DEFAULTS_EXPIRES_MINUTES), TimeSpan.Zero);

        BlobClient blob = new(new Uri(uri));

        var containerClient = blobServiceClient.GetBlobContainerClient(blob.BlobContainerName);
        var blobClient = containerClient.GetBlobClient(blob.Name);

        var blobIsExists = await blobClient.ExistsAsync(cancellationToken);

        if (!blobIsExists)
        {
            message = $"Blob not found. uri={uri}";
            logger.LogWarning("{message}", message);
            throw new ApiException(HttpStatusCode.NotFound, message);
        }

        if (!blobClient.CanGenerateSasUri)
        {
            message = "It can not generate SAS token from blob";
            logger.LogWarning("{message}", message);
            throw new ApiException(HttpStatusCode.Forbidden, message);
        }

        var builder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.BlobContainerName,
            Resource = "b",
        };

        if (canRead ?? false)
        {
            builder.SetPermissions(BlobSasPermissions.Read);
        }
        if (canWrite ?? false)
        {
            builder.SetPermissions(BlobSasPermissions.Write);
        }
        if (canDelete ?? false)
        {
            builder.SetPermissions(BlobSasPermissions.Delete);
        }
        if (canDeletePermanently ?? false)
        {
            builder.SetPermissions(BlobSasPermissions.PermanentDelete);
        }

        builder.ExpiresOn = expiresOnActual;

        var sasUri = blobClient.GenerateSasUri(builder);
        var uriWithSasToken = sasUri.ToString();

        logger.LogWarning("URI read access: {uri}", uriWithSasToken);

        return uriWithSasToken;
    }
}
