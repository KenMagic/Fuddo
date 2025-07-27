using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface IProductRepo
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);

        Task AddImageAsync(ProductImage image);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task DeleteImageAsync(int id);

        Task DeleteAllImagesAsync(int id);
    }
}
