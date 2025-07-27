using Fuddo.Models;

namespace Fuddo.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> CheckLoginAsync(string username, string password);
        Task<bool> UsernameExistsAsync(string username);
    }
}
