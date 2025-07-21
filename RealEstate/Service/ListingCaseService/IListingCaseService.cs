
﻿using RealEstate.DTOs.ListingCase;
using RealEstate.Enums;
﻿using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Service.ListingCaseService
{
    public interface IListingCaseService
    {
        Task<ListingCaseResponseDto> CreateListingCase(ListingCaseCreateRequest listingCaseCreateRequest);

        Task UpdateStatus(int caseId, ListcaseStatus newStatus, string userId, string role);


        Task<List<ListingWithStatusHistoryDto>> GetAllListingsAsync(string userId, string role);

        Task<ListingCaseUpdateResponseDto> UpdateListingCase(int id, ListingCaseUpdateRequestDto listingCaseUpdateRequest);

        Task<ListingCaseDetailResponseDto?> GetListingCaseDetailAsync(int id);
        Task<bool> AddAgentToListingCaseAsync(AddAgentToListingCaseDto dto);
        Task<bool> RemoveAgentFromListingCaseAsync(RemoveAgentFromListingCaseDto dto);
        Task<bool> DeleteListingCaseAsync(int id);
        Task<string> GenerateShareableLinkAsync(int id);
    }
}
