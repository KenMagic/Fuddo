using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Fuddo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService productService;
        private readonly ICategoryService categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            this.productService = productService;
            this.categoryService = categoryService;
        }

        public IActionResult Index()
        {
            var categories = categoryService.GetAllAsync().Result;
            var products = productService.GetAllAsync().Result;
            var availableProducts = products
               .Where(p => p.QuantityInStock > 0)
               .OrderByDescending(p => p.Id)
               .ToList();

            ViewBag.Categories = categories;
            return View(availableProducts);
        }
    }

}
