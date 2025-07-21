using RealEstate.DTOs.CaseContact;

namespace RealEstate.Service.CaseContactService
{
    public interface ICaseContactService
    {
        Task<CaseContactResponseDto> AddCaseContact(CaseContactRequestDto caseContactRequestDto);
        Task<List<CaseContactResponseDto>> GetCaseContactByLisitngCaseId(int id);
    }
}
