using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.SampleModel;

namespace react_native_app_bk.Services
{
    public interface ISampleService
    {
        Task<Sample> GetSample(int id);
        Task<IEnumerable<Sample>> GetAllSamples();
        Task<(bool Success, string ErrorMessage)> CreateSample(Sample sample);
    }
    public class SampleService : ISampleService
    {
        private readonly Data.AppDbContext _context;

        public SampleService(Data.AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sample> GetSample(int id)
        {
            var sample = await _context.Samples.FirstOrDefaultAsync(s => s.Id == id);

            if (sample == null)
            {
                throw new SampleNotFoundException($"No sample found with id: {id}");
            }

            return sample;
        }

        public async Task<IEnumerable<Sample>> GetAllSamples()
        {
            return await _context.Samples
                .Include(s => s.Sample_Type)
                .Include(s => s.Material)
                .Include(s => s.Test_Specimen_Type)
                .ToListAsync();
        }

        public async Task<(bool Success, string ErrorMessage)> CreateSample(Sample sample)
        {
            try
            {
                var validSampleType = await _context.Sample_Types.AnyAsync(st => st.Id == sample.Sample_Type_Id);
                if (!validSampleType)
                {
                    return (false, $"Invalid Sample Type Id: {sample.Sample_Type_Id}");
                }

                var validTestSpecimenType = await _context.Test_Specimen_Types.AnyAsync(tst => tst.Id == sample.Test_Specimen_Type_Id);
                if (!validTestSpecimenType)
                {
                    return (false, $"Invalid Test Specimen Type Id: {sample.Test_Specimen_Type_Id}");
                }

                var validMaterial = await _context.Materials.AnyAsync(m => m.Id == sample.Material_Id);
                if (!validMaterial)
                {
                    return (false, $"Invalid Material Id: {sample.Material_Id}");
                }

                _context.Samples.Add(sample);
                await _context.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

    }
    public class SampleNotFoundException : Exception
    {
        public SampleNotFoundException(string id) : base($"No sample found with id: {id}") { }
    }
}
