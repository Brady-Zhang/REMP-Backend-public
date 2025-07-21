using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using RealEstate.Service.AzureBlobStorage;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureBlobStorageController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public AzureBlobStorageController(IAzureBlobStorageService azureBlobStorageService)
        {
            _azureBlobStorageService = azureBlobStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(new { message = "No files received." });

            var allowedTypes = new[] { "image/", "video/", "application/vr", "model/" };
            foreach (var file in files)
            {
                var fileType = file.ContentType.ToLower();
                if (!allowedTypes.Any(type => fileType.StartsWith(type)))
                    return BadRequest(new { message = $"File type '{fileType}' is not allowed." });
            }
            var folderName = "test-uploads";

            try
            {
                var urls = await _azureBlobStorageService.UploadFilesAsync(files, folderName);
                return Ok(urls);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFiles([FromQuery]string blobUrl)
        {
            try
            {
                var (stream, contentType, fileName) = await _azureBlobStorageService.DownloadFileAsync(blobUrl);
                return File(stream, contentType, fileName);
            }
            catch(FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
