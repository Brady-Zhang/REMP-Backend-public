using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs;
using RealEstate.Enums;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RealEstate.Repository.ListingCaseRepository
{
    public class ListingCaseRepository : IListingCaseRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ListingCaseRepository> _logger;
        private readonly IMapper _mapper;

        public ListingCaseRepository(ApplicationDbContext applicationDbContext,
                                     IMongoDbContext mongoDbContext,
                                     IHttpContextAccessor httpContextAccessor,
                                     ILogger<ListingCaseRepository> logger,
                                     IMapper mapper)
        {
            _applicationDbContext = applicationDbContext;
            _mongoDbContext = mongoDbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task LogCaseHistoryAsync(IClientSessionHandle? session, ListingCase listingCase, string userId, ChangeAction action)
        {
            var caseHistory = new CaseHistory
            {
                ListingCaseId = listingCase.Id.ToString(),
                ChangeDetail = $"Listing Case is {action}.",
                ChangedBy = userId,
                ChangeDate = DateTime.Now,
                Action = action
            };
            if (session != null)
            {
                await _mongoDbContext.CaseHistories.InsertOneAsync(session, caseHistory);
            }
            else
            {
                await _mongoDbContext.CaseHistories.InsertOneAsync(caseHistory);
            }

        }
        public async Task LogStatusHistoryAsync(IClientSessionHandle? session, ListingCase listingCase, string userId, ListcaseStatus oldStatus, ListcaseStatus newStatus)
        {
            var statusHistory = new StatusHistory
            {
                ListingCaseId = listingCase.Id.ToString(),
                //OldStatus = "Hello",
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                UpdatedBy = userId,
                UpdatedAt = DateTime.Now
            };
            if (session != null)
            {
                await _mongoDbContext.StatusHistories.InsertOneAsync(session, statusHistory);
            }
            else
            {
                await _mongoDbContext.StatusHistories.InsertOneAsync(statusHistory);
            }

        }
        public async Task LogUserActivityAsync(IClientSessionHandle? session, ListingCase listingCase, string userId, UserActivityType type)
        {
            var userActivityLog = new UserActivityLog
            {
                UserId = userId,
                ActivityDetail = type.ToString(),
                ActivityDate = DateTime.Now
            };
            if (session != null)
            {
                await _mongoDbContext.UserActivityLogs.InsertOneAsync(session, userActivityLog);
            }
            else
            {
                await _mongoDbContext.UserActivityLogs.InsertOneAsync(userActivityLog);
            }

        }


        public async Task<ListingCase> CreateListingCase(ListingCase listingCase)
        {
            await _applicationDbContext.AddAsync(listingCase);
            await _applicationDbContext.SaveChangesAsync();
            return listingCase;
        }


        public async Task<ListcaseStatus> UpdateStatus(ListingCase listingCase, ListcaseStatus newStatus)
        {
            var oldStatus = listingCase.ListcaseStatus;
            listingCase.ListcaseStatus = newStatus;
            _applicationDbContext.Update(listingCase);
            await _applicationDbContext.SaveChangesAsync();
            return oldStatus;
        }

        public async Task<ListingCase?> GetListingCaseById(int caseId)
        {
            return await _applicationDbContext.ListingCases
                // Explicitly load related agent assignments; EF Core doesn't load navigation properties by default, 
                //which can cause false "unauthorized" errors
                .Include(x => x.AgentListingCases)  
                .FirstOrDefaultAsync(x => x.Id == caseId);
        }


        public IQueryable<ListingCase> GetAllListingCasesQueryable(string userId, string role)
        {
            IQueryable<ListingCase> query;

            switch (role)
            {
                case "Agent":
                    query = _applicationDbContext.ListingCases
                        .Include(x => x.AgentListingCases)
                        .Where(x => x.AgentListingCases.Any(a => a.AgentId == userId))
                        .AsQueryable();
                    break;
                case "PhotographyCompany":
                    query = _applicationDbContext.ListingCases
                        .Where(x => x.UserId == userId)
                        .AsQueryable();
                    break;
                case "Admin":
                    query = _applicationDbContext.ListingCases
                        .AsQueryable();
                    break;
                default:
                    throw new ArgumentException($"Unknown role: {role}", nameof(role));
            }

            return query;
        }


        public async Task<ListingCase> UpdateListingCase(int id, ListingCase updatedlistingCase)
        {
            var isIdExisting = await _applicationDbContext.ListingCases.FindAsync(id);

            await _applicationDbContext.SaveChangesAsync();

            return isIdExisting;
        }

        public async Task<ListingCase?> GetByIdAsync(int id)
        {
            return await _applicationDbContext.ListingCases
                .Include(l => l.MediaAssets)
                .Include(l => l.CaseContacts)
                .Include(l => l.AgentListingCases)
                    .ThenInclude(alc => alc.Agent)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        }

        public async Task<bool> AgentExistsInListingCaseAsync(string agentId, int listingCaseId)
        {
            return await _applicationDbContext.AgentListingCases
                .AnyAsync(alc => alc.AgentId == agentId && alc.ListingCaseId == listingCaseId);
        }

        public async Task<IEnumerable<Agent>> GetAgentsOfListingCaseAsync(int listingCaseId)
        {
            return await _applicationDbContext.AgentListingCases
                .Where(alc => alc.ListingCaseId == listingCaseId)
                .Include(alc => alc.Agent)
                .ThenInclude(a => a.User)
                .Select(alc => alc.Agent)
                .ToListAsync();
        }

        public async Task<bool> AddAgentToListingCaseAsync(AgentListingCase agentListingCase)
        {
            await _applicationDbContext.AgentListingCases.AddAsync(agentListingCase);
            return await _applicationDbContext.SaveChangesAsync() > 0;

        }
        public async Task<bool> RemoveAgentFromListingCaseAsync(string agentId, int listingCaseId)
        {
            var agentListingCase = await _applicationDbContext.AgentListingCases
                .FirstOrDefaultAsync(alc => alc.AgentId == agentId && alc.ListingCaseId == listingCaseId);

            if (agentListingCase == null)
            {
                return false;
            }

            _applicationDbContext.AgentListingCases.Remove(agentListingCase);
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteListingCaseAsync(int listingCaseId)
        {
            var listingCase = _applicationDbContext.ListingCases
                .FirstOrDefault(l => l.Id == listingCaseId);

            if (listingCase == null)
            {
                return false;
            }

            _applicationDbContext.ListingCases.Remove(listingCase);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
    }
}
