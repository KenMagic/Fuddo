using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fuddo.Controllers
{
    public class AdminUserController : Controller
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            int pageSize = 6;
            var allUsers = await _userService.GetAllAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                allUsers = allUsers
                    .Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(u.FullName) && u.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(u.Phone) && u.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    );
            }

            var pagedUsers = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCount = allUsers.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;

            return View(pagedUsers);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(User u)
        {
            if (ModelState.IsValid)
            {
                await _userService.AddAsync(u);
                TempData["Success"] = "Thêm người dùng thành công!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin người dùng.";
            return View(u);
        }
    }
}

