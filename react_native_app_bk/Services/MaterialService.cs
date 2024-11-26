using Microsoft.EntityFrameworkCore;
using react_native_app_bk.Models.MaterialModel;

namespace react_native_app_bk.Services
{
    public interface IMaterialService
    {
        Task<Material> GetMaterial(int id);
        Task<IEnumerable<Material>> GetAllMaterials();
        Task<(bool Success, string? ErrorMessage)> CreateMaterial(Material material);
        //Task<(bool Success, string ErrorMessage)> DeleteMaterial(int id);
    }
    public class MaterialService : IMaterialService
    {
        private readonly Data.AppDbContext _context;

        public MaterialService(Data.AppDbContext context)
        {
            _context = context;
        }

        public async Task<Material> GetMaterial(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                throw new MaterialNotFoundException($"No material found with id: {id}");
            }
            return material;
        }

        public async Task<IEnumerable<Material>> GetAllMaterials()
        {
            return await _context.Materials.ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateMaterial(Material material)
        {
            try
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
    }
    public class MaterialNotFoundException : Exception
    {
        public MaterialNotFoundException(string id) : base($"No material found with id: {id}") { }
    }
}
