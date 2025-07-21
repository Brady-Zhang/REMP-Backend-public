using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _client = new MongoClient(settings.Value.ConnectionString);
        _database = _client.GetDatabase(settings.Value.Database);
    }

    public IMongoClient Client
    {
        get { return _client; }
    }

    public IMongoCollection<CaseHistory> CaseHistories => _database.GetCollection<CaseHistory>("CaseHistory");
    public IMongoCollection<UserActivityLog> UserActivityLogs => _database.GetCollection<UserActivityLog>("UserActivityLog");
    public IMongoCollection<StatusHistory> StatusHistories => _database.GetCollection<StatusHistory>("StatusHistories");
    public IMongoCollection<UserRegisterHistory> UserRegistrationHistories => _database.GetCollection<UserRegisterHistory>("UserRegistrationHistories");

    public IMongoCollection<MediaDeletion> MediaDeletions => _database.GetCollection<MediaDeletion>("MediaDeletions");

    public IMongoCollection<SelectEvent> SelectEvents => _database.GetCollection<SelectEvent>("SelectEvents");

    public IMongoCollection<OrderHistory> OrderHistories => _database.GetCollection<OrderHistory>("OrderHistories");

}
