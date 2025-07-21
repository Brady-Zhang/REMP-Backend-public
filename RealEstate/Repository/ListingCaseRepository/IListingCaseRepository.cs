using MongoDB.Driver;
using RealEstate.Data;
﻿using Microsoft.AspNetCore.Mvc;

using RealEstate.Domain;
using RealEstate.DTOs;
using RealEstate.DTOs.ListingCase;
using RealEstate.Enums;
using System;

namespace RealEstate.Repository.ListingCaseRepository
{
    public interface IListingCaseRepository
    {
        Task<ListingCase> CreateListingCase(ListingCase listingCase);
        Task LogCaseHistoryAsync(IClientSessionHandle session, ListingCase listingCase, string userId, ChangeAction action);
        Task LogStatusHistoryAsync(IClientSessionHandle session, ListingCase listingCase, string userId, ListcaseStatus oldStatus, ListcaseStatus newStatus);
        Task LogUserActivityAsync(IClientSessionHandle session, ListingCase listingCase, string userId, UserActivityType type);

        Task<ListcaseStatus> UpdateStatus(ListingCase listingCase, ListcaseStatus newStatus);
        Task<ListingCase?> GetListingCaseById(int caseId);

        IQueryable<ListingCase> GetAllListingCasesQueryable(string userId, string role);
       

        Task<ListingCase> UpdateListingCase(int id, ListingCase updatedlistingCase);
        Task<ListingCase?> GetByIdAsync(int id);
        Task<bool> AgentExistsInListingCaseAsync(string agentId, int listingCaseId);
        Task<IEnumerable<Agent>> GetAgentsOfListingCaseAsync(int listingCaseId);
        Task<bool> AddAgentToListingCaseAsync(AgentListingCase agentListingCase);
        Task<bool> RemoveAgentFromListingCaseAsync(string agentId, int listingCaseId);
        Task<bool> DeleteListingCaseAsync(int id);
    }
}