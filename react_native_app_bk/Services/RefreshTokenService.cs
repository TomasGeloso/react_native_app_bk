using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Data;
using react_native_app_bk.Models.RefreshToken;

namespace react_native_app_bk.Services
{
    public interface IRefreshTokenService
    {
        Task AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshToken(int userId);
        Task DeleteRefreshToken(int id, string device);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshToken(int userId)
        {
            var response = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.User_Id == userId);

            return response;
        }

        public async Task DeleteRefreshToken(int userId, string device)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.User_Id == userId && rt.Device == device);
            if (refreshToken == null)
            {
                throw new RefreshTokenNotFoundException();
            }

            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }

    public class RefreshTokenNotFoundException : Exception
    {
        public RefreshTokenNotFoundException()
        : base("Refresh token not found") { }
    }
}
