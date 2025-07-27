using Fuddo.Models;

namespace Fuddo.Service.Interface
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(int userId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int productId, int quantity);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
        Task<bool> SaveAsync();
    }
}
