using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Service.Interface;

namespace Fuddo.Service.ImplV1
{
    public class CartServiceV1 : ICartService
    {
        private readonly ICartRepo _cartRepo;

        public CartServiceV1(ICartRepo cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                await _cartRepo.CreateCartAsync(userId);
                cart = await _cartRepo.GetCartByUserIdAsync(userId);
            }
            return cart!;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            return cart?.CartItems ?? new List<CartItem>();
        }

        public async Task<bool> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                return await _cartRepo.AddOrUpdateItemAsync(cart.Id, productId, quantity);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveItemAsync(int userId, int productId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            return await _cartRepo.RemoveItemAsync(cart.Id, productId);
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            return await _cartRepo.ClearCartAsync(cart.Id);
        }

        public async Task<bool> SaveAsync()
        {
            return await _cartRepo.SaveChangesAsync();
        }
    }
}
