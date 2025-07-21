using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RealEstate.Service.AzureBlobStorage
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        public readonly BlobServiceClient _blobServiceClient;
        public readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, string folderName)
        {
            try
            {
                var resultUrls = new List<string>();

                // get blob container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["AzureBlobStorage:ContainerName"]);
                await containerClient.CreateIfNotExistsAsync();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        throw new InvalidOperationException("File is empty.");

                    // blob name is unique                    
                    var blobName = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    await using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                    resultUrls.Add(blobClient.Uri.ToString());
                }

                return resultUrls;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading files to Azure Blob Storage");
                throw new Exception($"Error uploading files to Azure Blob Storage: {e.Message}", e);
            }
        }

        public async Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobClient = new BlobClient(uri);

            var blobExists = await blobClient.ExistsAsync();
            if (!blobExists)
            {
                throw new FileNotFoundException($"Blob not found....");
            }

            var result = await blobClient.DownloadAsync();

            var content = result.Value.Content;
            var contentType = result.Value.Details.ContentType;
            var fileName = Path.GetFileName(uri.LocalPath);

            return (content, contentType, fileName);
        }

        public async Task<bool> DeleteFileAsync(string blobUrl)
        {
            try 
            {
                var uri = new Uri(blobUrl);
                var blobClient = new BlobClient(uri);
                var blobExists = await blobClient.ExistsAsync();

                if (!blobExists)
                {
                    _logger.LogWarning($"Attempt to delete non-existent blob: {blobUrl}");
                    return false;
                }

                var response = await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
                _logger.LogInformation($"Blob deleted: {blobUrl}");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting file from Azure Blob Storage: {blobUrl}");
                throw new Exception($"Error deleting file from Azure Blob Storage: {e.Message}", e);
            }
        }
    }
}
