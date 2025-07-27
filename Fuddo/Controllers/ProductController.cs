using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Fuddo.Controllers
{
    public class ProductController : Controller
    {

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index(string? keyword, int? categoryId, decimal? maxPrice, int page = 1)
        {
            int pageSize = 6;

            var filteredProducts = await _productService.GetFilteredAsync(keyword, categoryId, maxPrice);
            var pagedProducts = filteredProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCount = filteredProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.MaxPrice = maxPrice;

            // Load categories from repo
            ViewBag.Categories = await _categoryService.GetAllAsync();

            return View(pagedProducts);
        }



        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productService.GetByIdAsync(id); // thêm await
            if (product == null) return NotFound();
            return View(product); // model đúng kiểu Product
        }



    }
}
