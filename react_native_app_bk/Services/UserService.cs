using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Data;
using react_native_app_bk.Models;

namespace react_native_app_bk.Services
{
    public class UserService : IUserService
    {
        private readonly Data.AppDbContext _context;

        public UserService(Data.AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task<bool> UsernameExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
