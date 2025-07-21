using RealEstate.Collection;

namespace RealEstate.Repository.StatusHistoryRepository
{
    public interface IStatusHistoryRepository
    {
        Task<List<StatusHistory>> GetByListingCaseIdsAsync(List<string> listingCaseIds);
    }
}
