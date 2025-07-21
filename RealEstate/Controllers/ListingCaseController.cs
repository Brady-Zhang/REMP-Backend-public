using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.ListingCase;
using RealEstate.Exceptions;
using RealEstate.Service.ListingCaseService;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingCaseController : ControllerBase
    {
        private readonly IListingCaseService _listingCaseService;
        private readonly ILogger<ListingCaseService> _logger;

        public ListingCaseController(IListingCaseService listingCaseService,
                                     ILogger<ListingCaseService> logger)
        {
            _listingCaseService = listingCaseService;
            _logger = logger;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("listings")]
        public async Task<IActionResult> CreateListingCase(ListingCaseCreateRequest listingCaseCreateRequest)
        {
            try
            {
                var result = await _listingCaseService.CreateListingCase(listingCaseCreateRequest);
                return Ok(result);
            }
            catch(Exception exception)
            {
                _logger.LogError(exception, "Error occurred while creating a listing case.");
                return StatusCode(500);

            }           
        }



        [HttpGet("listings")]
        // [Authorize]
        public async Task<IActionResult> GetListingsWithHistory()
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = user.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                {
                    _logger.LogWarning("Unauthorized access attempt.");
                    return Unauthorized("User not authenticated.");
                }

                var listings = await _listingCaseService.GetAllListingsAsync(userId, role);
                return Ok(listings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get listings with history.");
                return StatusCode(500, "An error occurred while retrieving the listings.");
            }
        }

        [HttpPatch("{id}/status")]
        //[Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ListingCaseStatusUpdateRequest request)
        {
       
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            _logger.LogInformation("Received status update request: ListingCaseId = {Id}, NewStatus = {Status}, UserId = {UserId}, Role = {Role}",
                           id, request.NewStatus, userId, role);

            try
            {
                await _listingCaseService.UpdateStatus(id, request.NewStatus, userId, role);
                _logger.LogInformation("Status updated successfully for ListingCaseId = {Id} by UserId = {UserId}", id, userId);
                return Ok(new { Message = "Status updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized status update attempt: ListingCaseId = {Id}, UserId = {UserId}, Role = {Role}",
                           id, userId, role);
                return StatusCode(StatusCodes.Status403Forbidden, new { Error = ex.Message });
            }
            catch (NotFoundException ex) 
            {
                _logger.LogWarning(ex, "Listing not found: ListingCaseId = {Id}, UserId = {UserId}", id, userId);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating status for ListingCaseId = {Id}", id);
                return BadRequest(new { Error = ex.Message });
            }
        }


        //[Authorize(Roles = "Admin")]
        [HttpPut("listings/{id}")]
        public async Task<IActionResult> UpdateListingCase(int id, [FromBody] ListingCaseUpdateRequestDto listingCaseUpdateRequest)
        {
            if (id <= 0)
            {
                return BadRequest("Listing case ID must be a positive integer.");
            }
            if (listingCaseUpdateRequest == null)
            {
                return BadRequest("Listing case update request cannot be null.");
            }
            try
            {
                var result = await _listingCaseService.UpdateListingCase(id, listingCaseUpdateRequest);
                return await GetListingCaseDetail(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError("Eorror occurred:" + ex);
                return NotFound("Listing case not found." + ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a listing case.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetListingCaseDetail(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Invalid listing ID."
                });

            var listing = await _listingCaseService.GetListingCaseDetailAsync(id);
            if (listing == null)
                return NotFound(new
                {
                    message = $"Listing case with ID {id} was not found."
                });

            return Ok(listing);
        }

        [HttpPost("AddAgentToListingCase")]
        public async Task<IActionResult> AddAgentToListingCase([FromBody] AddAgentToListingCaseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool result = await _listingCaseService.AddAgentToListingCaseAsync(dto);

                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Agent successfully added to listing case"
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to add agent to listing case"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpDelete("RemoveAgentFromListingCase")]
        public async Task<IActionResult> RemoveAgentFromListingCase([FromBody] RemoveAgentFromListingCaseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool result = await _listingCaseService.RemoveAgentFromListingCaseAsync(dto);
                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Agent successfully removed from listing case"
                    });
                }

                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to remove agent from listing case"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpDelete("RemoveListingCase")]
        public async Task<IActionResult> RemoveListingCase(int listingCaseId)
        {
            if (listingCaseId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid listing case ID."
                });
            }
            try
            {
                bool result = await _listingCaseService.DeleteListingCaseAsync(listingCaseId);
                if (result)
                {
                    return Ok(new
                    {
                        message = "Listing case deleted successfully."
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = "Listing case not found."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPost("{id}/generate-shareable-link")]

        public async Task<IActionResult> GenerateShareableLink(int id)
        {
            try
            {
                var link = await _listingCaseService.GenerateShareableLinkAsync(id);
                return Ok(new { shareableUrl = link });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating shareable link.");
                return StatusCode(500, "Internal server error.");
            }
        }


    }
}
