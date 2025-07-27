using Fuddo.Models;

namespace Fuddo.Service.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        Task<IEnumerable<Product>> GetFilteredAsync(string? keyword, int? categoryId, decimal? maxPrice);

        Task AddImageAsync(ProductImage image);
        Task DeleteImageAsync(int id);


    }
}
