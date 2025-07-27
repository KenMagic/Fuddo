using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Service.Interface;

namespace Fuddo.Service.ImplV1
{
    public class ProductServiceV1 : IProductService
    {
        private readonly IProductRepo _productRepo;
        public ProductServiceV1(IProductRepo productRepo)
        {
            _productRepo = productRepo;
        }
        public Task AddAsync(Product product)
        {
            return _productRepo.AddAsync(product);
        }

        public Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than zero.", nameof(id));
            Product? product = _productRepo.GetByIdAsync(id).Result;
            if(product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            // If product is in an order, throw an exception
            if (product.OrderDetails != null && product.OrderDetails.Any())
            {
                throw new InvalidOperationException("Cannot delete a product that is part of an order.");
            }
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                // Xóa tất cả hình ảnh liên quan đến sản phẩm
                foreach (var image in product.ProductImages)
                {
                    _productRepo.DeleteAllImagesAsync(image.Id).Wait();
                }
            }

            return _productRepo.DeleteAsync(id) ;

        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return _productRepo.GetAllAsync();
        }

        public Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return _productRepo.GetByIdAsync(id);
        }

        public Task UpdateAsync(Product product)
        {
            return _productRepo.UpdateAsync(product);
        }
        public async Task<IEnumerable<Product>> GetFilteredAsync(string? keyword, int? categoryId, decimal? maxPrice)
        {
            var allProducts = await _productRepo.GetAllAsync(); // Đã là List<Product>

            var filtered = allProducts;

            if (!string.IsNullOrWhiteSpace(keyword))
                filtered = filtered
                    .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (categoryId.HasValue && categoryId.Value > 0)
                filtered = filtered
                    .Where(p => p.CategoryId == categoryId.Value)
                    .ToList();

            if (maxPrice.HasValue && maxPrice.Value > 0)
                filtered = filtered
                    .Where(p => p.Price <= maxPrice.Value)
                    .ToList();

            return filtered;

        }

        public Task AddImageAsync(ProductImage image)
        {
            return _productRepo.AddImageAsync(image);
        }

        public Task DeleteImageAsync(int id)
        {
            return _productRepo.DeleteImageAsync(id);
        }
    }
}
