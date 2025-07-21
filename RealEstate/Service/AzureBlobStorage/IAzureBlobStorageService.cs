namespace RealEstate.Service.AzureBlobStorage
{
    public interface IAzureBlobStorageService
    {
        Task<List<string>> UploadFilesAsync(List<IFormFile> files, string folderName);
        Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(string blobUrl);
        Task<bool> DeleteFileAsync(string blobUrl);

    }
}
