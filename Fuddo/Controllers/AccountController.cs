using Fuddo.Models;
using Fuddo.Models.ViewModel;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fuddo.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Username,Password")] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _userService.CheckLoginAsync(model.Username, model.Password))
            {
                // Lấy user từ username
                var user = await _userService.GetByUsernameAsync(model.Username);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    //claim 
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);

                    return RedirectToAction("Index", "Home");
                }
                

            }


            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        //REGISTER
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string? email, string? fullname, string? phone, string? address)
        {
            var exists = await _userService.GetByUsernameAsync(username);
            if (exists != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            var user = new User
            {
                Username = username,
                PasswordHash = password,
                Email = email,
                FullName = fullname,
                Phone = phone,
                Address = address,
                Role = "User"
            };

            await _userService.AddAsync(user);

            // Chuyển về trang đăng nhập sau khi tạo tài khoản thành công
            return RedirectToAction("Login");
        }

        //LOGOUT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //PROFILE
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(User model)
        {
            var user = await _userService.GetByUsernameAsync(User.Identity?.Name);
            if (user == null) return RedirectToAction("Login");

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            await _userService.UpdateAsync(user); // Cần có phương thức UpdateAsync

            ViewBag.Success = "Cập nhật thông tin thành công!";
            return View(user);
        }

        //CHANGE PASSWORD
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var username = User.Identity?.Name;
            var user = await _userService.GetByUsernameAsync(username);

            if (user == null)
                return RedirectToAction("Login");

            var isValid = await _userService.CheckLoginAsync(user.Username, currentPassword);
            if (!isValid)
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            user.PasswordHash = newPassword;
            await _userService.UpdateAsync(user);

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

    }
}
