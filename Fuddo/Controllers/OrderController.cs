using Fuddo.Models;
using Fuddo.Models.ViewModel;
using Fuddo.Service.Interface;
using Fuddo.Services.Email;
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
        private readonly IMailService _mailService;

        public OrderController(ICartService cartService, IOrderService orderService, IUserService userService, IMailService mailService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userService = userService;
            _mailService = mailService;
        }

        public async Task<IActionResult> My(string status, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 5)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy tất cả order của user
            var orders = await _orderService.GetByUserIdAsync(user.Id);

            // Áp dụng filter
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status.ToString() == status);
            }

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value);
            }

            // Tổng đơn hàng sau khi filter
            int totalOrders = orders.Count();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var pagedOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Trả dữ liệu filter ra view để giữ giá trị
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(pagedOrders);
        }



        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Checkout", "Cart");

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItems = (await _cartService.GetCartItemsAsync(user.Id)).ToList();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Home");

            decimal totalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);

            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                UserId = user.Id,
                Address = model.Address,
                Phone = model.Phone,
                Note = model.Note,
                OrderDetails = cartItems.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList()
            };

            try
            {
                await _orderService.AddAsync(order);

                // Gửi email xác nhận
                var subject = $"Order Confirmation #{order.Id}";
                var body = $@"
            <h2>Thank you for your order, {model.FullName}!</h2>
            <p>Your order <strong>#{order.Id}</strong> has been placed successfully.</p>
            <p>Total Amount: <strong>{totalAmount:N0} ₫</strong></p>
            <p>Status: <strong>{order.Status}</strong></p>
            <p>We will contact you when the order is shipped.</p>";

                await _mailService.SendEmailAsync(user.Email, subject, body);

                // Xóa giỏ hàng
                await _cartService.ClearCartAsync(user.Id);
                await _cartService.SaveAsync();

                return RedirectToAction("ThankYou");
            }
            catch (Exception ex)
            {
                // Trả lỗi ra view
                TempData["Error"] = ex.Message;
                return RedirectToAction("Checkout","Cart");
            }
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


        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            // Chỉ cho phép hủy nếu chưa giao hàng
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
                return RedirectToAction("Details", new { id });
            }
            await _orderService.UpdateStatusAsync(order.Id, OrderStatus.Cancelled);

            // Gửi email thông báo hủy
            TempData["Success"] = "Đơn hàng đã được hủy.";
            return RedirectToAction("Details", new { id });
        }
    }
    }
