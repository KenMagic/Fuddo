using Fuddo.Models;
using Fuddo.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fuddo.Repository.SQLImpl
{
    public class PasswordResetTokenRepoSQL : IPasswordResetTokenRepo
    {
        private readonly FuddoContext _context;

        public PasswordResetTokenRepoSQL(FuddoContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow);
        }

        public async Task DeleteAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

    }

}
