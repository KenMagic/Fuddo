using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface IOrderRepo
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        Task AddAsync(Order order);
        Task UpdateStatusAsync(int orderId, OrderStatus newStatus);

        Task<Order?> GetWithDetailsAsync(int id);
    }
}
