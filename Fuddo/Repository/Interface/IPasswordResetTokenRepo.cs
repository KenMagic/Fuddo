using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface IPasswordResetTokenRepo
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task DeleteAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetByTokenAsync(string token);
    }

}
