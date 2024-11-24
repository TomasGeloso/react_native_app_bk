using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models;
using react_native_app_bk.Models.Sample;
using react_native_app_bk.Models.User;
using react_native_app_bk.Models.RefreshToken;

namespace react_native_app_bk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
            Users = Set<User>();
            Samples = Set<Sample>();
            SampleTypes = Set<SampleType>();
            Materials = Set<Material>();
            TestSpecimenTypes = Set<TestSpecimenType>();
            RefreshTokens = Set<RefreshToken>();
        }


        public DbSet<User> Users { get; set; }

        public DbSet<Sample> Samples { get; set; }
        public DbSet<SampleType> SampleTypes { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<TestSpecimenType> TestSpecimenTypes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
