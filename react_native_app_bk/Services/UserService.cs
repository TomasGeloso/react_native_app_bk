using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.User;

namespace react_native_app_bk.Services
{
    public interface IUserService
    {
        Task<bool> EmailExists(string email);
        Task<bool> UsernameExists(string username);
        Task<User> GetUserById(int id);
        Task<User> GetUserByEmail(string email);
        Task CreateUser(User user);
        Task UpdateUser(User user);
    }

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

        public async Task<User> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new UserNotFoundException(id.ToString());
            }
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new UserNotFoundException(email);
            }

            return user;
        }

        public async Task CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string email)
        : base($"No user found with email: {email}") { }
    }
}
