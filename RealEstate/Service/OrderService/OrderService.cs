using System;
using System.Security.Claims;
using System.Transactions;
using AutoMapper;
using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.Order;
using RealEstate.Enums;
using RealEstate.Repository.OrderRepository;
using ZstdSharp.Unsafe;

namespace RealEstate.Service.OrderService;

public class OrderService : IOrderService
{
  private readonly IOrderRepository _orderRepository;
  private readonly IMapper _mapper;
  private readonly IMongoDbContext _mongoDbContext;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<OrderService> _logger;

  public OrderService(IOrderRepository orderRepository, 
                      IMapper mapper, 
                      IMongoDbContext mongoDbContext, 
                      IHttpContextAccessor httpContextAccessor,
                      ILogger<OrderService> logger)
  {
    _orderRepository = orderRepository;
    _mapper = mapper;
    _mongoDbContext = mongoDbContext;
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  public async Task<OrderResponseDto> CreateOrder(OrderCreateRequest orderCreateRequest){
    var order = _mapper.Map<Order>(orderCreateRequest);
    order.CreatedAt = DateTime.UtcNow;
    order.UpdatedAt = null;
    order.Status = OrderStatus.Pending;

    string? userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogError("No valid user ID found when creating order");
        throw new InvalidOperationException("User ID is required to create an order");
    }

    try
            {
                // SQL Database transaction
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _orderRepository.CreateOrder(order);
                    scope.Complete();
                }
                 var responseDto = _mapper.Map<OrderResponseDto>(order);
                try
                {
                    // Define status values in service layer instead of repository
                    string oldStatus = "None";
                    string newStatus = OrderStatus.Pending.ToString();
                    // await _orderRepository.LogOrderHistoryAsync(null, order, userId, ChangeAction.Created);
                    await _orderRepository.LogOrderHistoryAsync(null, order, userId, oldStatus, newStatus);
                }
                catch (Exception logException)
                {
                    _logger.LogError(logException, "MongoDB logging failed for order creation");
                }
                
                return responseDto;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to create order");
                throw;
            }

  }


  public async Task<bool> VerifyMongoDbLoggingAsync(int orderId)
    {
        try
        {
            var filter = Builders<OrderHistory>.Filter.Eq("OrderId", orderId.ToString());
            var count = await _mongoDbContext.OrderHistories.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MongoDB logging");
            return false;
        }
    }


}
