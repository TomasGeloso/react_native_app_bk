using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.SampleTypeModel;

namespace react_native_app_bk.Services
{
    public interface ISampleTypeService
    {
        Task<SampleType> GetSampleType(int id);
        Task<IEnumerable<SampleType>> GetAllSampleTypes();
        Task<(bool Success, string? ErrorMessage)> CreateSampleType(SampleType sampleType);
        //Task<(bool Success, string ErrorMessage)> DeleteSampleType(int id);
    }
    public class SampleTypeService : ISampleTypeService
    {
        private readonly Data.AppDbContext _context;
        public SampleTypeService(Data.AppDbContext context)
        {
            _context = context;
        }
        public async Task<SampleType> GetSampleType(int id)
        {
            var sampleType = await _context.Sample_Types.FirstOrDefaultAsync(st => st.Id == id);
            if (sampleType == null)
            {
                throw new SampleTypeNotFoundException($"No sample type found with id: {id}");
            }
            return sampleType;
        }
        public async Task<IEnumerable<SampleType>> GetAllSampleTypes()
        {
            return await _context.Sample_Types.ToListAsync();
        }
        public async Task<(bool Success, string? ErrorMessage)> CreateSampleType(SampleType sampleType)
        {
            try
            {
                _context.Sample_Types.Add(sampleType);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
    }
    public class SampleTypeNotFoundException : Exception
    {
        public SampleTypeNotFoundException(string id) : base($"No sample type found with id: {id}") { }
    }
}
