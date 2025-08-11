namespace Fuddo.Service.Interface
{
    public interface IMailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendOrderStatusEmailAsync(string toEmail, string fullName, int orderId, string status, decimal totalAmount);

    }
}
