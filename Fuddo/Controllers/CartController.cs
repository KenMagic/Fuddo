using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fuddo.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IUserService _userService;

        public CartController(ICartService cartService, IUserService userService)
        {
            _cartService = cartService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập trước khi thêm vào giỏ hàng." });
            }

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            var result = await _cartService.AddToCartAsync(user.Id, productId, quantity);

            return Json(new
            {
                success = result,
                message = result ? "Đã thêm vào giỏ hàng!" : "Thêm vào giỏ hàng thất bại."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartPreview()
        {
            var username = User.Identity?.Name;
            var cartItems = new List<CartItem>();

            if (!string.IsNullOrEmpty(username))
            {
                var user = await _userService.GetByUsernameAsync(username);
                if (user != null)
                {
                    cartItems = (await _cartService.GetCartItemsAsync(user.Id)).ToList();
                }
            }

            return PartialView("_CartOffcanvas", cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _userService.GetByUsernameAsync(username);
                if (user != null)
                {
                    await _cartService.RemoveItemAsync(user.Id, productId);
                }
            }
            return Ok();
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            string username = User.Identity?.Name ?? "";

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = (await _cartService.GetCartItemsAsync(user.Id)).ToList();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.FullName = user.FullName ?? "";
            ViewBag.Phone = user.Phone ?? "";
            ViewBag.Address = user.Address ?? "";

            return View();
        }
    }
}
