using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RealEstate.DTOs.Order;
using RealEstate.Service.OrderService;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
      private readonly IOrderService _orderService;
      private readonly ILogger<OrderController> _logger;
      public OrderController(IOrderService orderService, ILogger<OrderController> logger)
      {
          _orderService = orderService;
          _logger = logger;
      }

        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderCreateRequest orderCreateRequest)
        {
            try
            {
                var result = await _orderService.CreateOrder(orderCreateRequest);
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while creating an order.");

                return StatusCode(500, new { 
                    error = "An error occurred while processing your request.",
                    details = exception.Message,
                    stack = exception.StackTrace,
                    innerError = exception.InnerException?.Message
                });
            }
        }

        // test mongoDB
        // [HttpGet("history/{orderId}")]
        //   public async Task<IActionResult> GetOrderHistory(string orderId)
        //   {
        //       try
        //       {
        //           var mongoDbContext = HttpContext.RequestServices.GetRequiredService<MongoDbContext>();
                  
        //           var orderHistories = await mongoDbContext.OrderHistories
        //               .Find(h => h.OrderId == orderId)
        //               .ToListAsync();
                      
        //           return Ok(orderHistories);
        //       }
        //       catch (Exception ex)
        //       {
        //           _logger.LogError(ex, "Error retrieving order history");
        //           return StatusCode(500, new { error = ex.Message });
        //       }
        //   }


    }
}
