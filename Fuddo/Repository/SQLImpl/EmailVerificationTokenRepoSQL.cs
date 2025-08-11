using Fuddo.Models;
using Fuddo.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Fuddo.Repository.SQLImpl
{
    public class EmailVerificationTokenRepoSQL: IEmailVerificationTokenRepo
    {
        private readonly FuddoContext _context;

        public EmailVerificationTokenRepoSQL(FuddoContext context)
        {
            _context = context;
        }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }

}
