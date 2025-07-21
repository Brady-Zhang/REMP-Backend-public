using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RealEstate.Collection;
using System;
using System.Threading.Tasks;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleDataController : ControllerBase
    {
        private readonly MongoDbContext _mongoContext;

        public SampleDataController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        [HttpGet("CreateSampleDataForMongoDB")]
        public async Task<IActionResult> CreateSampleDataForMongoDB()
        {
            try
            {
                var caseHistory1 = new CaseHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    ListingCaseId = "CASE-001",
                    ChangeDetail = "Property price updated from $450,000 to $475,000",
                    ChangedBy = "user123"
                };
                var caseHistory2 = new CaseHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    ListingCaseId = "CASE-002",
                    ChangeDetail = "Added new interior photos",
                    ChangedBy = "user456"
                };
                var userActivity1 = new UserActivityLog
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = "user123",
                    ActivityDetail = "Logged in from new device",
                };
                var userActivity2 = new UserActivityLog
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = "user456",
                    ActivityDetail = "Updated profile information",
                };

                var statusHistory1 = new StatusHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    ListingCaseId = "CASE-001",
                    OldStatus = "Pending",
                    NewStatus = "Active",
                    UpdatedBy = "user123"
                };

                var statusHistory2 = new StatusHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    ListingCaseId = "CASE-002",
                    OldStatus = "Draft",
                    NewStatus = "Pending",
                    UpdatedBy = "user456"
                };

                await _mongoContext.CaseHistories.InsertOneAsync(caseHistory1);
                await _mongoContext.CaseHistories.InsertOneAsync(caseHistory2);
                await _mongoContext.UserActivityLogs.InsertOneAsync(userActivity1);
                await _mongoContext.UserActivityLogs.InsertOneAsync(userActivity2);
                await _mongoContext.StatusHistories.InsertOneAsync(statusHistory1);
                await _mongoContext.StatusHistories.InsertOneAsync(statusHistory2);

                return Ok(new
                {
                    message = "Sample data added successfully",
                    caseHistories = new[] { caseHistory1, caseHistory2 },
                    userActivities = new[] { userActivity1, userActivity2 },
                    statusHistories = new[] { statusHistory1, statusHistory2 }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }


    }
}