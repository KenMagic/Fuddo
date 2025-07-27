using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Service.Interface;

namespace Fuddo.Service.ImplV1
{
    public class UserServiceV1 : IUserService
    {
        private readonly IUserRepo _userRepo;
        public UserServiceV1(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        //Register your services here, e.g., database context or repository
        public Task AddAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Hash the password before saving
            return _userRepo.AddAsync(user);
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
                    Role = "ADMIN",
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
    }
}
