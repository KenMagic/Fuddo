using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Service.Interface;
using Fuddo.Services.Email;
namespace Fuddo.Service.ImplV1
{
    public class OrderServiceV1 : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly IMailService _mailService;

        public OrderServiceV1(IOrderRepo orderRepo, IProductRepo productRepo, IMailService mailService)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _mailService = mailService;
        }
        public async Task AddAsync(Order order)
        {
            // 1. Kiểm tra order có chi tiết không
            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                throw new ArgumentException("Order must have at least one order detail.", nameof(order));
            }

            // 2. Kiểm tra số lượng và tồn kho từng sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                if (detail.Quantity <= 0)
                {
                    throw new ArgumentException("Order detail quantity must be greater than zero.", nameof(detail.Quantity));
                }

                var product = await _productRepo.GetByIdAsync(detail.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {detail.ProductId} not found.");
                }

                if (detail.Quantity > product.QuantityInStock)
                {
                    throw new InvalidOperationException(
                        $"Đã hết hàng {product.Name}. Còn: {product.QuantityInStock}, Đặt: {detail.Quantity}.");
                }

                // 3. Trừ tồn kho
                product.QuantityInStock -= detail.Quantity;
                await _productRepo.UpdateAsync(product);
            }

            // 4. Lưu đơn hàng
            await _orderRepo.AddAsync(order);
        }

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            return _orderRepo.GetAllAsync();
        }

        public Task<Order?> GetByIdAsync(int id)
        {
            return _orderRepo.GetByIdAsync(id);
        }

        public Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return _orderRepo.GetByUserIdAsync(userId);
        }

        public Task<Order?> GetWithDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found.");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("This order is already completed or cancelled, cannot update status.");
            if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Product.QuantityInStock += detail.Quantity;
                }
            }
            await _orderRepo.UpdateStatusAsync(orderId, newStatus);

            await _mailService.SendOrderStatusEmailAsync(
                order.User.Email,
                order.User.FullName,
                order.Id,
                newStatus.ToString(),
                order.TotalAmount
            );
        }


    }
}
