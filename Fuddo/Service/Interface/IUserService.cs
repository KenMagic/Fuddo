using Fuddo.Models;

namespace Fuddo.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> CheckLoginAsync(string username, string password);
        Task<bool> UsernameExistsAsync(string username);
        Task SendPasswordResetLinkAsync(User user, string baseUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<PasswordResetToken?> ValidateResetTokenAsync(string token);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> VerifyEmailAsync(string token);
        Task SendVerificationEmailAsync(User user, string baseUrl);
        Task<User?> GetByPhoneAsync(string phone);

    }
}
