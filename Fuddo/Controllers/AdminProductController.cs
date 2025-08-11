using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fuddo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            int pageSize = 6;
            var allProducts = await _productService.GetAllAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                allProducts = allProducts
                    .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var pagedProducts = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCount = allProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;

            return View(pagedProducts);
        }



        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync(); // hoặc async
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product p, IFormFile[] newImages)
        {
            if (ModelState.IsValid)
            {
                await _productService.AddAsync(p);

                if (newImages != null && newImages.Length > 0)
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    foreach (var file in newImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var newImage = new ProductImage
                            {
                                ProductId = p.Id, // ID được gán sau khi AddAsync()
                                ImageUrl = "/images/uploads/" + fileName
                            };

                            await _productService.AddImageAsync(newImage);
                        }
                    }
                }

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories;

            TempData["Error"] = "Vui lòng kiểm tra lại thông tin sản phẩm.";
            return View(p);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _categoryService.GetAllAsync(); 
            return View(product);
        }


        [HttpPost]
        
        public async Task<IActionResult> Edit(Product p, IFormFile[] newImages, List<int> deleteImageIds)
        {
            if (ModelState.IsValid)
            {
                // Cập nhật thông tin chính
                await _productService.UpdateAsync(p);
                if (deleteImageIds != null && deleteImageIds.Any())
                {
                    foreach (var imageId in deleteImageIds)
                    {
                        await _productService.DeleteImageAsync(imageId);
                    }
                }
                // Xử lý ảnh mới nếu có
                if (newImages != null && newImages.Length > 0)
                {
                    foreach (var file in newImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Lưu đường dẫn vào DB
                            var newImage = new ProductImage
                            {
                                Product = p,
                                ProductId = p.Id,
                                ImageUrl = "/images/uploads/" + fileName
                                
                            };
                            await _productService.AddImageAsync(newImage);
                        }
                    }
                }
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ
            foreach (var entry in ModelState)
            {
                var key = entry.Key;
                var errors = entry.Value.Errors;

                foreach (var error in errors)
                {
                    Console.WriteLine($"ModelState Error: {key} - {error.ErrorMessage}");
                }
            }
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories;

            var existing = await _productService.GetByIdAsync(p.Id);
            p.ProductImages = existing?.ProductImages ?? new List<ProductImage>();

            TempData["Error"] = "Cập nhật sản phẩm" + p.Name +" không thành công. Vui lòng kiểm tra lại thông tin.";
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
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
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

    }
}
