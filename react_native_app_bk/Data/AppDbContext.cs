using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models;

namespace react_native_app_bk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Here we define the tables like DbSet

        public DbSet<User> Users { get; set; }

    }
}
