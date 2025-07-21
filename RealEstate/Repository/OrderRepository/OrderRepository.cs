using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.Enums;

namespace RealEstate.Repository.OrderRepository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ApplicationDbContext applicationDbContext, 
                             IMongoDbContext mongoDbContext, 
                             IHttpContextAccessor httpContextAccessor,
                             ILogger<OrderRepository> logger)
        {
            _applicationDbContext = applicationDbContext;
            _mongoDbContext = mongoDbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            await _applicationDbContext.Orders.AddAsync(order);
            await _applicationDbContext.SaveChangesAsync();
            return order;
        }

        // public async Task LogOrderHistoryAsync(IClientSessionHandle? session, Order order, string userId, ChangeAction action)
        public async Task LogOrderHistoryAsync(IClientSessionHandle? session, Order order, string userId, string oldStatus, string newStatus)
        {
            var orderHistory = new OrderHistory
             {
                OrderId = order.Id.ToString(),
                // OldStatus = action == ChangeAction.Created ? "None" : order.Status.ToString(),
                // NewStatus = action == ChangeAction.Created ? OrderStatus.Pending.ToString() : order.Status.ToString(),
                OldStatus = oldStatus,
                NewStatus = newStatus,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };
            
            if (session != null)
            {
                await _mongoDbContext.OrderHistories.InsertOneAsync(session, orderHistory);
            }
            else
            {
                await _mongoDbContext.OrderHistories.InsertOneAsync(orderHistory);
            }
        }
    }
}