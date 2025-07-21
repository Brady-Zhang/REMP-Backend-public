using System;
using MongoDB.Driver;
using RealEstate.Domain;
using RealEstate.DTOs.Order;

namespace RealEstate.Service.OrderService;

public interface IOrderService
{
  Task<OrderResponseDto> CreateOrder(OrderCreateRequest orderCreateRequest);
 
}
