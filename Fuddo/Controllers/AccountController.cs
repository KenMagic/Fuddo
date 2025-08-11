using Fuddo.Models;
using Fuddo.Models.ViewModel;
using Fuddo.Repository.Interface;
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
                return View(model);
            if (await _userService.CheckLoginAsync(model.Username, model.Password))
            {
                var user = await _userService.GetByUsernameAsync(model.Username);
                if (user != null)
                {
                    // Kiểm tra xác minh email
                    if (!user.IsVerified && !user.Username.Equals("admin"))
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn chưa được xác minh email. Vui lòng kiểm tra email.");
                        var baseUrl = $"{Request.Scheme}://{Request.Host}";
                        await _userService.SendVerificationEmailAsync(user, baseUrl);
                        return View(model);
                    }

                    // Set session & cookie
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserRole", user.Role);

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
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra email đã tồn tại
            var emailExists = await _userService.GetByEmailAsync(model.Email);
            if (emailExists != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Kiểm tra username đã tồn tại
            var exists = await _userService.GetByUsernameAsync(model.Username);
            if (exists != null)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                Role = "User"
            };

            await _userService.AddAsync(user);

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
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            var resetToken = await _userService.ValidateResetTokenAsync(token);
            if (resetToken == null)
            {
                // Hiển thị trang thông báo token không hợp lệ hoặc hết hạn
                return BadRequest("InvalidToken");
            }

            // Truyền token hợp lệ sang view
            return View(new ResetPasswordViewModel { Token = token });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Gọi service xử lý reset
                await _userService.ResetPasswordAsync(model.Token, model.NewPassword);

                TempData["Success"] = "Password reset successfully. Please login.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Tìm user theo email
            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Message"] = "If the email exists, a reset link has been sent.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Tạo và gửi mail reset password
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _userService.SendPasswordResetLinkAsync(user, baseUrl);

            return RedirectToAction("ForgotPasswordConfirmation");
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var success = await _userService.VerifyEmailAsync(token);

            if (!success)
            {
                ViewBag.Message = "Verification link is invalid or expired. Please request a new verification email.";
                return View("VerificationFailed");
            }

            ViewBag.Message = "Your email has been verified successfully!";
            return View("VerificationSuccess");
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
