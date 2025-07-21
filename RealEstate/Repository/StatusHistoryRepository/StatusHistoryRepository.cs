using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;

namespace RealEstate.Repository.StatusHistoryRepository
{
    public class StatusHistoryRepository : IStatusHistoryRepository
    {
        private readonly IMongoDbContext _mongoDbContext;

        public StatusHistoryRepository(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task<List<StatusHistory>> GetByListingCaseIdsAsync(List<string> listingCaseIds)
        {
            var filter = Builders<StatusHistory>.Filter.In(x => x.ListingCaseId, listingCaseIds);
            return await _mongoDbContext.StatusHistories.Find(filter).ToListAsync();
        }
    }
}
