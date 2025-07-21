
﻿using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Collection;
using RealEstate.Controllers;
using Microsoft.EntityFrameworkCore.Storage;
using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.User;
using System.Security.Cryptography;
using System;

namespace RealEstate.Repository.UserRepository
{
    public class UserRepository: IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly IMongoDbContext _mongoDbContext;
        private UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        public UserRepository(UserManager<User> userManager, IMongoDbContext mongoDbContext, ILogger<UserRepository> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _mongoDbContext = mongoDbContext;
            _logger = logger;
            _context = context;
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
        }
   
        public async Task<Agent> CreateAgentAsync(Agent agent)
        {
            await _context.Agents.AddAsync(agent);
            await _context.SaveChangesAsync();
            return agent;
        }
        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Agent> UpdateAgentAsync(Agent agent)
        {
            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();
            return agent;
        }

        public async Task DeleteAgentAsync(Agent agent)
        {
            _context.Agents.Remove(agent);
            await _context.SaveChangesAsync();
        }


        public async Task<PhotographyCompany> CreatePhotographyCompanyAsync(PhotographyCompany pc)
        {
            await _context.PhotographyCompanies.AddAsync(pc);
            await _context.SaveChangesAsync();
            return pc;
        }

        public async Task<IdentityResult> UserRegisterAddRoleAsync(User user, string role)
        {
            try
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (roleResult == null)
                {
                    _logger.LogWarning("User manager can not add user role.");
                }
                return roleResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing user role create method in UserRegisterRepository");
                throw;
            }
        }


        public async Task<User> UserRegisterRepositoryAsync(User user,string password)
        {
            try
            {
                //try to create user
                var result = await _userManager.CreateAsync(user, password);

                //create user register history in mongo db
                var newUserRegisterHistory = new UserRegisterHistory
                {
                    UserName = user.UserName,
                    EventDate = DateTime.Now,
                };
                if (result == null)
                {
                    _logger.LogWarning("User manager can not add user to database.");
                    //set user register status
                    newUserRegisterHistory.Status = "Failed";
                    await _mongoDbContext.UserRegistrationHistories.InsertOneAsync(newUserRegisterHistory);
                }
                else
                {
                    _logger.LogInformation("Add user to database successfully.");
                    //set user register status
                    newUserRegisterHistory.Status = "Succeed";
                    await _mongoDbContext.UserRegistrationHistories.InsertOneAsync(newUserRegisterHistory);
                }
                return result.Succeeded ? user : null;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error while processing create user method in UserRegisterRepository");
                throw ;
            }
        }  

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.ListingCases)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.ListingCases)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {           
            try
            {
                return await _userManager.Users
                    .Include(u => u.AgentProfile)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower()); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when finding agent by email: {email}");
            }
            return null;
        }

        public async Task<IEnumerable<Agent>> SearchAgentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _context.Agents.Take(20).Include(a => a.User).ToListAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Agents
                .Include(a => a.User)
                .Where(a =>
                    (a.AgentFirstName != null && a.AgentFirstName.ToLower().Contains(searchTerm)) ||
                    (a.AgentLastName != null && a.AgentLastName.ToLower().Contains(searchTerm)) ||
                    (a.CompanyName != null && a.CompanyName.ToLower().Contains(searchTerm)) ||
                    (a.User.Email != null && a.User.Email.ToLower().Contains(searchTerm)) ||
                    (a.User.PhoneNumber != null && a.User.PhoneNumber.Contains(searchTerm))
                )
                .ToListAsync();
        }

        public async Task<bool> IsEmailExist(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            return existingUser != null;
        }


        public async Task<bool> UserIsAgent(string userId)
        { 
            try
            {
                return await _context.Agents.AnyAsync(a => a.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is agent in Agent table: {userId}");
                return false;
            }
        }

        public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
        {
            return await _context.Agents
                .Include(a => a.User)
                .Where(a => !a.User.IsDeleted)
                .ToListAsync();
        }

        public async Task<Agent> GetAgentByIdAsync(string id)
        {
            return await _context.Agents
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && !a.User.IsDeleted);
        }

        public async Task<IEnumerable<PhotographyCompany>> GetAllPhotographyCompanyAsync()
        {
            return await _context.PhotographyCompanies
                .Include(pc => pc.User)
                .ToListAsync();
        }

        public async Task<PhotographyCompany> GetPhotographyCompanyByIdAsync(string id)
        {
            return await _context.PhotographyCompanies
                .Include(pc => pc.User)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<IdentityResult> UpdatePasswordAsync(User user , string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        
    }
    
}
