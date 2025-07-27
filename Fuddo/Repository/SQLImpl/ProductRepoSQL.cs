using Fuddo.Models;
using Fuddo.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Fuddo.Repository.SQLImpl
{
    public class ProductRepoSQL : IProductRepo
    {
        private readonly FuddoContext _context;

        public ProductRepoSQL(FuddoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetWithImagesAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddImageAsync(ProductImage image)
        {
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllImagesAsync(int id)
        {
            _context.ProductImages.RemoveRange(_context.ProductImages.Where(pi => pi.ProductId == id));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }
}
