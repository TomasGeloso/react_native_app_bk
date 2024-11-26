using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.SampleModel;
using react_native_app_bk.Models.UserModel;
using react_native_app_bk.Models.RefreshTokenModel;
using react_native_app_bk.Models.MaterialModel;
using react_native_app_bk.Models.SampleTypeModel;
using react_native_app_bk.Models.TestSpecimenTypeModel;

namespace react_native_app_bk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
            Users = Set<User>();
            Samples = Set<Sample>();
            Sample_Types = Set<SampleType>();
            Materials = Set<Material>();
            Test_Specimen_Types = Set<TestSpecimenType>();
            RefreshTokens = Set<RefreshToken>();
        }


        public DbSet<User> Users { get; set; }

        public DbSet<Sample> Samples { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<SampleType> Sample_Types { get; set; }
        public DbSet<TestSpecimenType> Test_Specimen_Types { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
