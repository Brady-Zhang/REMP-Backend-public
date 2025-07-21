using AutoMapper;
using RealEstate.Domain;
using RealEstate.DTOs.CaseContact;
using RealEstate.Exceptions;
using RealEstate.Repository.CaseContactRepository;

namespace RealEstate.Service.CaseContactService
{
    public class CaseContactService : ICaseContactService
    {
        private readonly ICaseContactRepository _caseContactRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CaseContactService> _logger;

        public CaseContactService(ICaseContactRepository caseContactRepository,
                                  IMapper mapper,
                                  ILogger<CaseContactService> logger)
        {
            _caseContactRepository = caseContactRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<CaseContactResponseDto> AddCaseContact(CaseContactRequestDto caseContactRequestDto)
        {
            var caseContact = _mapper.Map<CaseContact>(caseContactRequestDto);
            try
            {
                var result = await _caseContactRepository.AddCaseContact(caseContact);
                if(result == null)
                {
                    throw new CaseContactCreationException("Failed to add CaseContact...");
                }
                return _mapper.Map<CaseContactResponseDto>(result);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to add CaseContact....");
                throw;
            }        
        }

        public async Task<List<CaseContactResponseDto>> GetCaseContactByLisitngCaseId(int id)
        {
            try
            {
                var result = await _caseContactRepository.GetCaseContactByLisitngCaseId(id);
                return _mapper.Map<List<CaseContactResponseDto>>(result);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to get CaseContacts....");
                throw;
            }
        }
    }
}
