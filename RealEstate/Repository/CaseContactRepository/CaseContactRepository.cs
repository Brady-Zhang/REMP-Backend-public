using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.Exceptions;

namespace RealEstate.Repository.CaseContactRepository
{
    public class CaseContactRepository : ICaseContactRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<CaseContactRepository> _logger;

        public CaseContactRepository(ApplicationDbContext applicationDbContext, 
                                     ILogger<CaseContactRepository> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task<CaseContact?> AddCaseContact(CaseContact caseContact)
        { 
            try
            {
                var listingCaseExists = await ListingCaseExist(caseContact.ListingCaseId);
                if (!listingCaseExists)
                {
                    _logger.LogWarning($"ListingCaseId {caseContact.ListingCaseId} not found.");
                    return null;
                }
                _applicationDbContext.CaseContacts.Add(caseContact);
                await _applicationDbContext.SaveChangesAsync();
                return caseContact;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to add CaseContact...");
                return null;
            } 
        }

        public async Task<bool> ListingCaseExist (int id)
        {
            try
            {
                return await _applicationDbContext.ListingCases.AnyAsync(l => l.Id == id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"ListingCase with id {id} doesn't exist.");
                return false;
            }
        }

        public async Task<List<CaseContact>?> GetCaseContactByLisitngCaseId(int id)
        {
            try
            {
                var listingCaseExists = await ListingCaseExist(id);
                if (!listingCaseExists)
                {
                    throw new NotFoundException($"ListingCaseId {id} not found.");
                }
                return await _applicationDbContext.CaseContacts.Where(c => c.ListingCaseId == id).ToListAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to get CaseContacts...");
                return null;
            }
        }
    }
}
