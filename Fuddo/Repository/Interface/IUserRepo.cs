using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface IUserRepo
    {
        Task<IEnumerable<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);

        Task<User?> GetByUsernameAsync(string username);

        Task<User> AddAsync(User user);

        Task UpdateAsync(User user);

        // Xóa user (cho admin)
        Task DeleteAsync(int id);

        Task<bool> CheckLoginAsync(string username, string password);

        Task<bool> UsernameExistsAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);

    }
}
