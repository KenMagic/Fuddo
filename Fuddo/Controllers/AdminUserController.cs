using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fuddo.Controllers
{
    [Authorize(Roles = "Admin")]

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
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.PasswordHash = GenerateRandomPassword();
                    var createdUser =  await _userService.AddAsync(user);
                    await _userService.SendPasswordResetLinkAsync(createdUser, $"{Request.Scheme}://{Request.Host}");

                    TempData["Success"] = "Tạo người dùng thành công và đã gửi email!";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index");
                }

                // Cập nhật role
                user.Role = role;

                await _userService.UpdateAsync(user);

                TempData["Success"] = $"Đã cập nhật quyền người dùng thành {role}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        public string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}

