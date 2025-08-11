using Fuddo.Service.Email;
using Fuddo.Service.Interface;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Runtime;
using System.Threading.Tasks;

namespace Fuddo.Services.Email
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            using var client = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.Port)
            {
                Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password),
                EnableSsl = _mailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_mailSettings.SenderEmail, _mailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
        public async Task SendOrderStatusEmailAsync(string toEmail, string fullName, int orderId, string status, decimal totalAmount)
        {
            var subject = $"[Fuddo] Your Order #{orderId} is now {status}";
            var body = BuildOrderStatusHtml(fullName, orderId, status, totalAmount);

            using var client = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.Port)
            {
                Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password),
                EnableSsl = _mailSettings.EnableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_mailSettings.SenderEmail, _mailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }

        private string BuildOrderStatusHtml(string fullName, int orderId, string status, decimal totalAmount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background-color: #fff;
            border-radius: 10px;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        h2 {{
            color: #FF6F00;
            text-align: center;
        }}
        .status {{
            font-size: 18px;
            font-weight: bold;
            color: #2196F3;
            text-transform: uppercase;
            margin: 10px 0;
            text-align: center;
        }}
        .total {{
            font-size: 20px;
            color: #E53935;
            text-align: center;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #777;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Thank you for shopping at Fuddo!</h2>
        <p>Hi <strong>{fullName}</strong>,</p>
        <p>Your order <strong>#{orderId}</strong> has been updated to:</p>
        <div class='status'>{status}</div>
        <div class='total'>Total: {totalAmount:N0} ₫</div>
        <p>We will notify you with further updates.</p>
        <div class='footer'>This is an automated message from Fuddo Shop.</div>
    </div>
</body>
</html>";
        }
    }
}

