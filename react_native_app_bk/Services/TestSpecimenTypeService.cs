using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.TestSpecimenTypeModel;

namespace react_native_app_bk.Services
{
    public interface ITestSpecimenTypeService
    {
        Task<TestSpecimenType> GetTestSpecimenType(int id);
        Task<IEnumerable<TestSpecimenType>> GetAllTestSpecimenTypes();
        Task<(bool Success, string? ErrorMessage)> CreateTestSpecimenType(TestSpecimenType testSpecimenType);
        //Task<(bool Success, string ErrorMessage)> DeleteTestSpecimenType(int id);
    }
    public class TestSpecimenTypeService : ITestSpecimenTypeService
    {
        private readonly Data.AppDbContext _context;

        public TestSpecimenTypeService(Data.AppDbContext context)
        {
            _context = context;
        }

        public async Task<TestSpecimenType> GetTestSpecimenType(int id)
        {
            var testSpecimenType = await _context.Test_Specimen_Types.FirstOrDefaultAsync(tst => tst.Id == id);
            if (testSpecimenType == null)
            {
                throw new TestSpecimenTypeNotFoundException($"No test specimen type found with id: {id}");
            }
            return testSpecimenType;
        }

        public async Task<IEnumerable<TestSpecimenType>> GetAllTestSpecimenTypes()
        {
            return await _context.Test_Specimen_Types.ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateTestSpecimenType(TestSpecimenType testSpecimenType)
        {
            try
            {
                _context.Test_Specimen_Types.Add(testSpecimenType);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
    }

    public class TestSpecimenTypeNotFoundException : Exception
    {
        public TestSpecimenTypeNotFoundException(string id) : base($"No test specimen type found with id: {id}")
        {
        }
    }
}
