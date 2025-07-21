using MongoDB.Driver;
using RealEstate.Collection;

namespace RealEstate.Data
{

    public interface IMongoDbContext
    {
        IMongoCollection<CaseHistory> CaseHistories { get; }
        IMongoCollection<UserActivityLog> UserActivityLogs { get; }
        IMongoCollection<StatusHistory> StatusHistories { get; }
        IMongoCollection<UserRegisterHistory> UserRegistrationHistories { get; }
        IMongoCollection<MediaDeletion> MediaDeletions { get; }
        IMongoCollection<SelectEvent> SelectEvents { get; }
        IMongoCollection<OrderHistory> OrderHistories { get; }

        IMongoClient Client { get; } // 可选：如果业务中直接用到
    }
    
}
