using RealEstate.Collection;
using RealEstate.Data;

namespace RealEstate.Service.LoggerService
{
    public class MongoLoggerService : ILoggerService
    {
        private readonly IMongoDbContext _mongo;

        public MongoLoggerService(IMongoDbContext mongo)
        {
            _mongo = mongo;
        }

        public async Task LogUserActivityAsync(string userId, string detail)
        {
            await _mongo.UserActivityLogs.InsertOneAsync(new UserActivityLog
            {
                UserId = userId,
                ActivityDetail = detail,
                ActivityDate = DateTime.UtcNow
            });
        }

        public async Task LogListingChangeAsync(string listingId, string detail, string userId)
        {
            await _mongo.CaseHistories.InsertOneAsync(new CaseHistory
            {
                ListingCaseId = listingId,
                ChangeDetail = detail,
                ChangedBy = userId,
                ChangeDate = DateTime.UtcNow
            });
        }

        public async Task LogStatusChangeAsync(string listingId, string oldStatus, string newStatus, string userId)
        {
            await _mongo.StatusHistories.InsertOneAsync(new StatusHistory
            {
                ListingCaseId = listingId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
