namespace RealEstate.Service.LoggerService
{
    public interface ILoggerService
    {
        Task LogUserActivityAsync(string userId, string detail);
        Task LogListingChangeAsync(string listingId, string detail, string userId);
        Task LogStatusChangeAsync(string listingId, string oldStatus, string newStatus, string userId);
    }
}
