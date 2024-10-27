using react_native_app_bk.Models;

namespace react_native_app_bk.Services
{
    public interface IUserService
    {
        Task<bool> EmailExists(string email);
        Task<User> GetUserByEmail(string email);
        Task CreateUser(User user);
    }
}
