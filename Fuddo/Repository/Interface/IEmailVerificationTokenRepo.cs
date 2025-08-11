using Fuddo.Models;

namespace Fuddo.Repository.Interface
{
    public interface IEmailVerificationTokenRepo
    {
        Task AddAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetByTokenAsync(string token);
        Task DeleteAsync(EmailVerificationToken token);
    }
}
