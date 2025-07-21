using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Domain;
using RealEstate.DTOs.CaseContact;
using RealEstate.Exceptions;
using RealEstate.Service.CaseContactService;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseContactController : ControllerBase
    {
        private readonly ICaseContactService _caseContactService;
        private ILogger<CaseContactController> _logger;

        public CaseContactController(ICaseContactService caseContactService, 
                                     ILogger<CaseContactController> logger)
        {
            _caseContactService = caseContactService;
            _logger = logger;
        }

        [HttpPost("caseContact")]
        public async Task<IActionResult> AddCaseContact(CaseContactRequestDto caseContact)
        {
            var result = await _caseContactService.AddCaseContact(caseContact);
            try
            { 
                if (result == null)
                {
                    return NotFound($"Listing Id {caseContact.ListingCaseId} not found....");
                }
                return Ok(result);
            }
            catch(CaseContactCreationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to add CaseContact with listingId {caseContact.ListingCaseId}");
                return StatusCode(500, new
                {
                    Message = "Something went wrong."
                });
            }
        }

        [HttpGet("caseContacts/{id}")]
        public async Task<IActionResult> GetCaseContactByLisitngCaseId(int id)
        {
            try
            {
                var result = await _caseContactService.GetCaseContactByLisitngCaseId(id);
                return Ok(result);
            }
            catch(NotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get CaseContacts with listingId {id}");
                return StatusCode(500, new
                {
                    Message = "Something went wrong..."
                });
            }
        }
    }
}
