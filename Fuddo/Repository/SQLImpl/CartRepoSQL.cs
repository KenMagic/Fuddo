using Fuddo.Models;
using Fuddo.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Fuddo.Repository.SQLImpl
{
    public class CartRepoSQL : ICartRepo
    {
        private readonly FuddoContext _context;

        public CartRepoSQL(FuddoContext context)
        {
            _context = context;
        }

        // CART

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> CreateCartAsync(int userId)
        {
            try
            {
                var cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveCartAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                return await SaveChangesAsync();
            }
            return false;
        }

        // CART ITEM

        public async Task<IEnumerable<CartItem>> GetItemsAsync(int cartId)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                .Where(i => i.CartId == cartId)
                .ToListAsync();
        }

        public async Task<CartItem?> GetItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
        }

        public async Task<bool> AddOrUpdateItemAsync(int cartId, int productId, int quantity)
        {
            try
            {
                var item = await GetItemAsync(cartId, productId);
                if (item != null)
                {
                    item.Quantity += quantity;
                    _context.CartItems.Update(item);
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    await _context.CartItems.AddAsync(newItem);
                }
                return await SaveChangesAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveItemAsync(int cartId, int productId)
        {
            var item = await GetItemAsync(cartId, productId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                return await SaveChangesAsync();
            }
            return false;
        }

        public async Task<bool> ClearCartAsync(int cartId)
        {
            try
            {
                var items = await _context.CartItems
                    .Where(i => i.CartId == cartId)
                    .ToListAsync();
                _context.CartItems.RemoveRange(items);
                return await SaveChangesAsync();
            }
            catch
            {
                return false;
            }
        }

        // SAVE

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
