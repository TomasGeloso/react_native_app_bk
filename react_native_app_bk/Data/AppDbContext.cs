using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models;
using react_native_app_bk.Models.Sample;
using react_native_app_bk.Models.User;

namespace react_native_app_bk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Sample> Samples { get; set; } = null!;
        public DbSet<SampleType> SampleTypes { get; set; } = null!;
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<TestSpecimenType> TestSpecimenTypes { get; set; } = null!;

    }
}
