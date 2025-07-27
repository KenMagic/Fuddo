using Fuddo.Models;
using Fuddo.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Fuddo.Controllers
{
    public class AdminOrderController : Controller
    {
        private readonly IOrderService _orderService;

        private readonly IProductService _productService;

        public AdminOrderController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }
        public async Task<IActionResult> Index(string? keyword, string? status, bool today = false, int page = 1)
        {
            int pageSize = 10;
            var orders = await _orderService.GetAllAsync();

            // 1. Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(keyword))
            {
                orders = orders.Where(o =>
                    o.User.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status)
                && !status.Equals("all", StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<OrderStatus>(status, true, out OrderStatus statusEnum))
            {
                // Nếu chuyển đổi thành công, sử dụng biến enum để lọc
                // Đây là phép so sánh enum-với-enum, rất an toàn và hiệu quả
                orders = orders.Where(o => o.Status == statusEnum);
            }

            // 3. Lọc theo hôm nay
            if (today)
            {
                var todayDate = DateTime.Today;
                orders = orders.Where(o => o.OrderDate.Date == todayDate);
            }

            // Phân trang
            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)orders.Count() / pageSize);
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Today = today;

            return View(pagedOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();
            ViewBag.NextStatus = GetNextStatus(order.Status);
            return View(order);
        }

        private OrderStatus? GetNextStatus(OrderStatus current)
        {
            return current switch
            {
                OrderStatus.Pending => OrderStatus.Processing,
                OrderStatus.Processing => OrderStatus.Shipped,
                OrderStatus.Shipped => OrderStatus.Delivered,
                _ => null 
            };
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            try
            {
                newStatus = (OrderStatus)GetNextStatus(newStatus);
                await _orderService.UpdateStatusAsync(orderId, newStatus);
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message; // Không tìm thấy order
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // Không cho update trạng thái
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
            }

            return RedirectToAction("Details", new { id = orderId });
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

            // Hoàn kho: cộng lại số lượng sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                var product = await _productService.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    product.QuantityInStock += detail.Quantity;
                    await _productService.UpdateAsync(product);
                }
            }

            // Gửi email thông báo hủy
            TempData["Success"] = "Đơn hàng đã được hủy.";
            return RedirectToAction("Details", new { id });
        }


    }
}
