
﻿using Microsoft.AspNetCore.Identity;
using RealEstate.Domain;
using RealEstate.DTOs.User;


namespace RealEstate.Repository.UserRepository
{
    public interface IUserRepository
    {

        Task<User> UserRegisterRepositoryAsync(User user,string password);
        Task<IdentityResult> UserRegisterAddRoleAsync(User user, string role);
        Task<User?> FindUserByEmailAsync(string email);
        Task<IEnumerable<Agent>> SearchAgentsAsync(string searchTerm);
        Task<bool> IsEmailExist(string email);
        Task<bool> UserIsAgent(string userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(string userId);
        Task DeleteAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<Agent> CreateAgentAsync(Agent agent);
        Task<Agent> UpdateAgentAsync(Agent agent);
        Task DeleteAgentAsync(Agent agent);
        Task<PhotographyCompany> CreatePhotographyCompanyAsync(PhotographyCompany pc);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IEnumerable<Agent>> GetAllAgentsAsync();
        Task<Agent> GetAgentByIdAsync(string id);
        Task<IEnumerable<PhotographyCompany>> GetAllPhotographyCompanyAsync();
        Task<PhotographyCompany> GetPhotographyCompanyByIdAsync(string id);

        Task<IdentityResult> UpdatePasswordAsync(User user, string currentPassword, string newPassword);
       
        
    }

}
