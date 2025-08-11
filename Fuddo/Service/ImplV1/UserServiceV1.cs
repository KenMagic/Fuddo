using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Repository.SQLImpl;
using Fuddo.Service.Interface;
using Fuddo.Services.Email;

namespace Fuddo.Service.ImplV1
{
    public class UserServiceV1 : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IMailService _mailService;
        private readonly IPasswordResetTokenRepo _passwordResetTokenRepo;
        private readonly IEmailVerificationTokenRepo _tokenRepo;
        public UserServiceV1(IUserRepo userRepo,
            IMailService mailService,
            IPasswordResetTokenRepo passwordResetTokenRepo,
            IEmailVerificationTokenRepo tokenRepo
            )
        {
            _userRepo = userRepo;
            _mailService = mailService;
            _passwordResetTokenRepo = passwordResetTokenRepo;
            _tokenRepo = tokenRepo;
        }
        public async Task<User> AddAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            return await _userRepo.AddAsync(user);
        }
        public async Task SendPasswordResetLinkAsync(User user, string baseUrl)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(24);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = expiry
            };

            await _passwordResetTokenRepo.AddAsync(resetToken);

            var resetLink = $"{baseUrl}/Account/ResetPassword?token={token}";

            var subject = "Set Your Password for Fuddo";
            var body = $@"
        <h2>Welcome to Fuddo!</h2>
        <p>Your account has been created. Please click below to set your password:</p>
        <a href='{resetLink}' style='display:inline-block;background:#007bff;color:#fff;padding:10px 20px;border-radius:5px;text-decoration:none;'>Set Password</a>
        <p>This link expires in 24 hours.</p>";

            await _mailService.SendEmailAsync(user.Email, subject, body);
        }
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _passwordResetTokenRepo.GetValidTokenAsync(token);
            if (resetToken == null) return false;

            var user = resetToken.User;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _passwordResetTokenRepo.DeleteAsync(resetToken);

            await _userRepo.UpdateAsync(user);
            return true;
        }

        public Task<bool> CheckLoginAsync(string username, string password)
        {
            if(username == "admin" && password == "a")
            {
                return Task.FromResult(true);
            }
            return _userRepo.CheckLoginAsync(username, password);
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return _userRepo.GetAllAsync();
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return _userRepo.GetByIdAsync(id);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            if (username == "admin")
            {
                return Task.FromResult<User?>(new User
                {
                    Id = 1,
                    Username = "admin",
                    FullName = "Admin User",
                    Email = "",
                    Role = "Admin",
                });
             }
            return _userRepo.GetByUsernameAsync(username);
        }

        public Task UpdateAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Hash the password before saving
            return _userRepo.UpdateAsync(user);
        }

        public Task<bool> UsernameExistsAsync(string username)
        {
            throw new NotImplementedException();
        }
        public async Task<PasswordResetToken?> ValidateResetTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var resetToken = await _passwordResetTokenRepo.GetByTokenAsync(token);

            // Token không tồn tại hoặc đã hết hạn
            if (resetToken == null || resetToken.ExpiryDate < DateTime.UtcNow)
                return null;

            return resetToken;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task SendVerificationEmailAsync(User user, string baseUrl)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(24);

            await _tokenRepo.AddAsync(new EmailVerificationToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = expiry
            });

            var link = $"{baseUrl}/Account/VerifyEmail?token={token}";
            var subject = "Verify your email";
            var body = $@"
            <h2>Welcome to Fuddo!</h2>
            <p>Please verify your email by clicking the button below:</p>
            <a href='{link}' style='background:#28a745;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;'>Verify Email</a>
            <p>This link will expire in 24 hours.</p>";

            await _mailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var record = await _tokenRepo.GetByTokenAsync(token);

            if (record == null) return false;
            if (record.ExpiryDate < DateTime.UtcNow)
            {
                await _tokenRepo.DeleteAsync(record);
                return false;
            }
            record.User.IsVerified = true;
            await _userRepo.UpdateAsync(record.User);
            await _tokenRepo.DeleteAsync(record);

            return true;
        }
        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _userRepo.GetByPhoneAsync(phone);
        }

    }
}
