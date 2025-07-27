using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface ICartRepo
    {
        // CART
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<bool> CreateCartAsync(int userId);
        Task<bool> RemoveCartAsync(int cartId);

        // CART ITEM
        Task<IEnumerable<CartItem>> GetItemsAsync(int cartId);
        Task<CartItem?> GetItemAsync(int cartId, int productId);
        Task<bool> AddOrUpdateItemAsync(int cartId, int productId, int quantity);
        Task<bool> RemoveItemAsync(int cartId, int productId);
        Task<bool> ClearCartAsync(int cartId);

        // SAVE
        Task<bool> SaveChangesAsync();
    }
}
