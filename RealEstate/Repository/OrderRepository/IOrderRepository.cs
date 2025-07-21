using MongoDB.Driver;
using RealEstate.Domain;
using RealEstate.Enums;

namespace RealEstate.Repository.OrderRepository
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(Order order);
         Task LogOrderHistoryAsync(IClientSessionHandle? session, Order order, string userId, string oldStatus, string newStatus);
    }
}