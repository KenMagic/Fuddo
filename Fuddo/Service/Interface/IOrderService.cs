using Fuddo.Models;

namespace Fuddo.Service.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        Task AddAsync(Order order);
        Task UpdateStatusAsync(int orderId, OrderStatus newStatus);

        Task<Order?> GetWithDetailsAsync(int id);
    }
}
