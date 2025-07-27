using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fuddo.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminCategoryController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            int pageSize = 6;

            // Lấy tất cả category
            var allCategories = await _categoryService.GetAllAsync();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(keyword))
            {
                allCategories = allCategories
                    .Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            // Phân trang
            var pagedCategories = allCategories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCount = allCategories.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Gửi dữ liệu phân trang cho View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;

            return View(pagedCategories);
        }



        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.AddAsync(category);
                    TempData["Success"] = "Thêm danh mục thành công!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
            }

            TempData["Error"] = "Không thể thêm danh mục. Vui lòng kiểm tra lỗi.";
            return View(category);
        }



        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }



        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateAsync(category);
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex) // Ví dụ trùng tên
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
            }

            TempData["Error"] = "Cập nhật danh mục không thành công. Vui lòng kiểm tra lại thông tin.";
            return View(category);
        }




        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }


    }
}
