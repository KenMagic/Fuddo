using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fuddo.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public OrderController(ICartService cartService, IOrderService orderService, IUserService userService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> My(int page = 1, int pageSize = 5)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return RedirectToAction("Login", "Account");

            var allOrders = await _orderService.GetByUserIdAsync(user.Id);
            int totalOrders = allOrders.Count();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var pagedOrders = allOrders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedOrders);
        }


        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string fullName, string address, string phone, string note)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = (await _cartService.GetCartItemsAsync(user.Id)).ToList();
            if (!cartItems.Any()) return RedirectToAction("Index", "Home");

            // Tính tổng tiền
            decimal totalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);

            // Tạo đơn hàng
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                UserId = user.Id,
                Address = address,
                Phone = phone,
                Note = note,
                OrderDetails = cartItems.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList()
            };

            await _orderService.AddAsync(order);

            // Xóa giỏ hàng
            await _cartService.ClearCartAsync(user.Id);
            await _cartService.SaveAsync();

            return RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou()
        {
            return View();
        }

        //ỎDER DETAILS
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            // Kiểm tra quyền sở hữu đơn hàng
            var username = User.Identity?.Name;
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null || order.UserId != user.Id)
                return Forbid();

            return View(order);
        }

    }
}
