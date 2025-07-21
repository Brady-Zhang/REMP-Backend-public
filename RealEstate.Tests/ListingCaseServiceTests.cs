using AutoMapper;
using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.ListingCase;
using RealEstate.Enums;
using RealEstate.Exceptions;
using RealEstate.Repository.ListingCaseRepository;
using RealEstate.Repository.StatusHistoryRepository;
using RealEstate.Service.Email;
using RealEstate.Service.ListingCaseService;
using System.Security.Claims;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace RealEstate.Tests
{
    public class ListingCaseServiceTests
    {
        private readonly ListingCaseService _listingCaseService;
        private readonly Mock<IListingCaseRepository> _listingCaseRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IMongoDbContext> _mongoContext;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ILogger<ListingCaseService>> _logger;
        private readonly Mock<IStatusHistoryRepository> _statusHistoryRepository;
        private readonly Mock<IEmailAdvancedSender> _smtpEmailSender;
        private readonly Mock<IConfiguration> _configuration;
        private readonly ApplicationDbContext _dbContext;

        public ListingCaseServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Setup mocks
            _listingCaseRepository = new Mock<IListingCaseRepository>();
            _mapper = new Mock<IMapper>();
            _mongoContext = new Mock<IMongoDbContext>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _logger = new Mock<ILogger<ListingCaseService>>();
            _statusHistoryRepository = new Mock<IStatusHistoryRepository>();
            _smtpEmailSender = new Mock<IEmailAdvancedSender>();
            _configuration = new Mock<IConfiguration>();

            // Create service with mocks
            _listingCaseService = new ListingCaseService(
                _listingCaseRepository.Object,
                _mapper.Object,
                _mongoContext.Object,
                _httpContextAccessor.Object,
                _dbContext,
                _logger.Object,
                _statusHistoryRepository.Object,
                _smtpEmailSender.Object,
                _configuration.Object
            );
        }

        [Fact]
        public async Task UpdateStatus_ValidCall_UpdatesStatusAndSendsEmailAndLogs()
        {
            // Arrange
            var listing = new ListingCase
            {
                Id = 1,
                Title = "Test Listing",
                ListcaseStatus = ListcaseStatus.Created,
                User = new User { Email = "listing-owner@example.com" }
            };

            var agent = new Agent
            {
                Id = "agent-1",
                User = new User { Email = "agent@example.com" }
            };

            var relation = new AgentListingCase
            {
                ListingCaseId = listing.Id,
                AgentId = agent.Id,
                Agent = agent,
                ListingCase = listing
            };

            _dbContext.ListingCases.Add(listing);
            _dbContext.Agents.Add(agent);
            _dbContext.AgentListingCases.Add(relation);
            _dbContext.SaveChanges();

            // mock GetListingCaseById
            _listingCaseRepository.Setup(r => r.GetListingCaseById(1))
                .ReturnsAsync(listing);

            // mock UpdateStatus 并附带状态更改逻辑
            _listingCaseRepository.Setup(r => r.UpdateStatus(listing, ListcaseStatus.Pending))
                .Callback(() => listing.ListcaseStatus = ListcaseStatus.Pending)
                .ReturnsAsync(ListcaseStatus.Pending);

            // mock GetAgents
            _listingCaseRepository.Setup(r => r.GetAgentsOfListingCaseAsync(1))
                .ReturnsAsync(new List<Agent> { agent });

            // mock 日志记录
            _listingCaseRepository.Setup(r =>
                r.LogStatusHistoryAsync(
                    It.IsAny<IClientSessionHandle>(),
                    listing,
                    "user123",
                    ListcaseStatus.Created,
                    ListcaseStatus.Pending)
            ).Returns(Task.CompletedTask);// 表示这是一个成功完成的异步操作

            // mock 前端配置和邮件发送
            _configuration.Setup(c => c["FrontendUrl"]).Returns("http://test-frontend.com");

            _smtpEmailSender.Setup(s => s.SendEmailAsync(
                agent.User.Email,
                It.IsAny<string>(),
                It.IsAny<string>())
            ).Returns(Task.CompletedTask);

            // Act
            await _listingCaseService.UpdateStatus(1, ListcaseStatus.Pending, "user123", "Admin");

            // Assert 状态被修改
            Assert.Equal(ListcaseStatus.Pending, listing.ListcaseStatus);

            // Assert 邮件发送
            _smtpEmailSender.Verify(sender =>
                sender.SendEmailAsync(
                    "agent@example.com",
                    "Your ListingCase Status Has Changed",
                    It.Is<string>(html => html.Contains("Listing Status Updated"))),
                Times.Once);

            // Assert 日志被调用 
            _listingCaseRepository.Verify(r =>//验证被测代码是否实际调用了某个方法，并且传入了指定参数
                r.LogStatusHistoryAsync(
                    null,
                    listing,
                    "user123",
                    ListcaseStatus.Created,
                    ListcaseStatus.Pending),
                Times.Once);
        }
        [Fact]
        public async Task UpdateStatus_WhenCalledByAgent_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var listing = new ListingCase
            {
                Id = 1,
                Title = "Test Listing",
                ListcaseStatus = ListcaseStatus.Created,
                User = new User { Email = "listing-owner@example.com" }
            };

            _listingCaseRepository.Setup(r => r.GetListingCaseById(1))
                .ReturnsAsync(listing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _listingCaseService.UpdateStatus(1, ListcaseStatus.Pending, "user123", "Agent"));

            Assert.Equal("Agents cannot change the status.", ex.Message);
        }
        [Fact]
        public async Task UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
        {
            // Arrange
            var listing = new ListingCase
            {
                Id = 1,
                Title = "Test Listing",
                ListcaseStatus = ListcaseStatus.Created,
                User = new User { Email = "listing-owner@example.com" }
            };

            _listingCaseRepository.Setup(r => r.GetListingCaseById(1))
                .ReturnsAsync(listing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _listingCaseService.UpdateStatus(1, ListcaseStatus.Delivered, "user123", "Admin"));

            Assert.StartsWith("Invalid status transition", ex.Message);
        }
        [Fact]
        public async Task UpdateStatus_ListingNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _listingCaseRepository.Setup(r => r.GetListingCaseById(99))
                .ReturnsAsync((ListingCase)null!); // 模拟查不到数据

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _listingCaseService.UpdateStatus(99, ListcaseStatus.Pending, "user123", "Admin"));

            Assert.Equal("Listing not found", ex.Message);
        }



        [Fact]
        public async Task UpdateListingCase_ValidUpdate_UpdatesFieldsAndLogs()
        {
            // Arrange
            var userId = "user-123";
            var caseId = 1;

            // 创建假用户身份
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var existing = new ListingCase
            {
                Id = caseId,
                Title = "Old Title",
                Description = "Old Description",
                Postcode = 2000,
                Price = 1000000,
                User = new User { Id = userId, Email = "user@example.com" }
            };

            _dbContext.ListingCases.Add(existing);
            _dbContext.SaveChanges();

            var request = new ListingCaseUpdateRequestDto
            {
                Title = "New Title",
                Description = "New Description",
                Postcode = 3000,
                Price = 1500000
            };

            _listingCaseRepository.Setup(r => r.GetByIdAsync(caseId))
                .ReturnsAsync(existing);

            _listingCaseRepository.Setup(r => r.UpdateListingCase(caseId, It.IsAny<ListingCase>()))
                .ReturnsAsync(existing);

            _mapper.Setup(m => m.Map<ListingCaseUpdateResponseDto>(It.IsAny<ListingCase>()))
                .Returns(new ListingCaseUpdateResponseDto { Title = "New Title" });

            // mock mongo log
            _listingCaseRepository.Setup(r =>
                r.LogCaseHistoryAsync(
                    null,
                    existing,
                    userId,
                    ChangeAction.Updated)).Returns(Task.CompletedTask);

            _listingCaseRepository.Setup(r =>
                r.LogUserActivityAsync(
                    null,
                    existing,
                    userId,
                    UserActivityType.LoggedIn)).Returns(Task.CompletedTask);

            // Act
            var result = await _listingCaseService.UpdateListingCase(caseId, request);

            // Assert
            Assert.Equal("New Title", existing.Title);
            Assert.Equal("New Description", existing.Description);
            Assert.Equal(3000, existing.Postcode);
            Assert.Equal(1500000, existing.Price);
            Assert.Equal("New Title", result.Title);

            _listingCaseRepository.Verify(r => r.UpdateListingCase(caseId, existing), Times.Once);
            _listingCaseRepository.Verify(r =>
                r.LogCaseHistoryAsync(null, existing, userId, ChangeAction.Updated), Times.Once);
            _listingCaseRepository.Verify(r =>
                r.LogUserActivityAsync(null, existing, userId, UserActivityType.LoggedIn), Times.Once);
        }
        [Fact]
        public async Task UpdateListingCase_UserNotLoggedIn_ThrowsException()
        {
            // Arrange
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext()); // 无用户

            var request = new ListingCaseUpdateRequestDto { Title = "Anything" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _listingCaseService.UpdateListingCase(1, request));

            Assert.Equal("UserId not found", ex.Message);
        }
        [Fact]
        public async Task UpdateListingCase_ListingNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123")
            }, "mock"));

            _httpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

            _listingCaseRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ListingCase)null!);

            var request = new ListingCaseUpdateRequestDto { Title = "Doesn't matter" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _listingCaseService.UpdateListingCase(999, request));

            Assert.Contains("not found", ex.Message);
        }


    }
}
