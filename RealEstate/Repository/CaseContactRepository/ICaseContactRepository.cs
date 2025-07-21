using RealEstate.Domain;

namespace RealEstate.Repository.CaseContactRepository
{
    public interface ICaseContactRepository
    {
        Task<CaseContact?> AddCaseContact(CaseContact caseContact);
        Task<List<CaseContact>?> GetCaseContactByLisitngCaseId(int id);
    }
}
