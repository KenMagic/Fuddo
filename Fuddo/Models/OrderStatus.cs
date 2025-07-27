namespace Fuddo.Models
{
    public enum OrderStatus
    {
        Pending,      // Đang chờ xử lý
        Processing,   // Đang xử lý
        Shipped,      // Đã giao hàng
        Delivered,    // Đã giao thành công
        Cancelled,    // Đã hủy
        Failed        // Thất bại
    }
}