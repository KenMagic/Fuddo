using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace Fuddo.Service.ImplV1
{
    public class CategoryServiceV1 : ICategoryService
    {

        private readonly ICategoryRepository _cateRepo;

        public CategoryServiceV1(ICategoryRepository cateRepo)
        {
            _cateRepo = cateRepo;
        }
        public async Task AddAsync(Category category)
        {
            var existing = await _cateRepo.GetAllAsync();
            if (existing.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Danh mục '{category.Name}' đã tồn tại.");

            // Gọi repository để lưu
            await _cateRepo.AddAsync(category);
        }


        public async Task DeleteAsync(int id)
        {
            // 1. Kiểm tra ID hợp lệ
            if (id <= 0)
                throw new ArgumentException("ID phải lớn hơn 0.", nameof(id));

            // 2. Lấy category
            var category = await _cateRepo.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Danh mục với ID {id} không tồn tại.");

            if (category.Products != null && category.Products.Any())
                throw new InvalidOperationException("Không thể xóa danh mục vì đang chứa sản phẩm.");
            // 4. Xóa category
            await _cateRepo.DeleteAsync(id);
        }


        public Task<IEnumerable<Category>> GetAllAsync()
        {
            return _cateRepo.GetAllAsync();
        }

        public Task<Category?> GetByIdAsync(int id)
        {
            return _cateRepo.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Category category)
        {
            var all = await _cateRepo.GetAllAsync();
            if (all.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase) && c.Id != category.Id))
                throw new InvalidOperationException($"Danh mục '{category.Name}' đã tồn tại.");

            await _cateRepo.UpdateAsync(category);
        }

    }
}
