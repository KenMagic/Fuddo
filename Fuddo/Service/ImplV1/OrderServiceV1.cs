using Fuddo.Models;
using Fuddo.Service.Interface;

using Fuddo.Repository.Interface;
namespace Fuddo.Service.ImplV1
{
    public class OrderServiceV1 : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;

        public OrderServiceV1(IOrderRepo orderRepo, IProductRepo productRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
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
                        $"Insufficient stock for product ID {detail.ProductId}. Available: {product.QuantityInStock}, Requested: {detail.Quantity}.");
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
                throw new KeyNotFoundException($"Đơn hàng {orderId} không tồn tại.");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Đơn hàng đã hoàn tất hoặc đã hủy, không thể cập nhật.");

            await _orderRepo.UpdateStatusAsync(orderId, newStatus);
        }

    }
}
