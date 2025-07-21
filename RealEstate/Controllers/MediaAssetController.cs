using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Exceptions;
using RealEstate.Repository;
using System.Security.Claims;
using RealEstate.DTOs.MediaAsset;
using RealEstate.Service.MediaAssetService;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaAssetController : ControllerBase
    {
        private readonly IMediaAssetService _mediaAssetService;
        private readonly ILogger<MediaAssetController> _logger;

        public MediaAssetController(IMediaAssetService mediaAssetService, ILogger<MediaAssetController> logger)
        {
            _mediaAssetService = mediaAssetService;
            _logger = logger;
        }

        /// <summary>
        /// Get all media assets for a specific listing.
        /// Only Admin or assigned Agent can access.
        /// </summary>
        /// <param name="id">Listing Case ID</param>
        /// <returns>List of media asset DTOs</returns>
        [HttpGet("{id}/media")]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<ActionResult<List<MediaAssetResponseDto>>> GetMediaAssetsByListingIdAsync(int id)
        {
            try
            {
                var result = await _mediaAssetService.GetMediaAssetsByListingIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Listing case {ListingId} not found.", id);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to listing {ListingId} media.", id);
                return StatusCode(403, "You are not authorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media for listing {ListingId}.", id);
                return StatusCode(500, "An error occurred while retrieving media assets.");
            }
        }

        [HttpDelete("{id}/delete")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteMediaById(int id)
        {
            try
            {
                var result = await _mediaAssetService.DeleteMediaById(id);
                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Media not found or already deleted."
                    });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteMediaById");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize(Roles = "Agent")]
        [HttpPatch("listings/{id}/select")]
        public async Task<IActionResult> SelectMediaByAgent([FromRoute] int id, [FromBody] MediaSelectDto mediaSelectDto)
        {

            try
            {
                var result = await _mediaAssetService.SelectMediaByAgent(id, mediaSelectDto);
                return Ok(result);
            }
            catch (MediaSelectLimitExceedException exception)
            {
                return BadRequest("You can only select up to 10 media files per listing.");
            }

        }
        [HttpGet("GetAgentMedia")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetMediaAssetsById(string agentId)
        {
            try
            {
                var result = await _mediaAssetService.GetMediaAssetsByAgentId(agentId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogWarning(exception, "Access denied");
                return StatusCode(403, "Not authorized");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error retrieving media for agentId");
                return StatusCode(500, "An error occurred while retrieving media assets.");
            }
        }

        [HttpPost("uploadMediaToListingCase")]
        public async Task<IActionResult> UploadMediaToListingCaseId([FromForm] UploadMediaRequest request)
        {
            var files = request.Files;
            var type = request.Type;
            var listingCaseId = request.ListingCaseId;

            if (files == null || !files.Any())
                return BadRequest(new { message = "No files received." });

            var allowedTypes = new[] { "image/", "video/", "application/vr", "model/" };

            foreach (var file in files)
            {
                var fileType = file.ContentType.ToLower();
                if (!allowedTypes.Any(type => fileType.StartsWith(type)))
                    return BadRequest(new { message = $"File type '{fileType}' is not allowed." });
            }
            try
            {
                var result = await _mediaAssetService.UploadMediaToListingCaseId(request);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No media assets uploaded." });
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation failed in media upload...");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload media failed.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("downloadMedia/{id}")]
        public async Task<IActionResult> DownloadMediaById(int id)
        {
            try
            {
                var (stream, contentType, fileName) = await _mediaAssetService.DownloadMediaAssetById(id);
                return File(stream, contentType, fileName);
            }
            catch(FileNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while downloading media...");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

    }
}


