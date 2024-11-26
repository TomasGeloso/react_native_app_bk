using Microsoft.AspNetCore.Mvc;
using react_native_app_bk.Services;
using react_native_app_bk.Models.MaterialModel;
using Microsoft.AspNetCore.Authorization;

namespace react_native_app_bk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMaterialService materialService, ILogger<MaterialController> logger)
        {
            _materialService = materialService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                IEnumerable<Material> materials = await _materialService.GetAllMaterials();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all materials.");
                return StatusCode(500, "An error occurred while retrieving all materials.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMaterial(int id)
        {
            try
            {
                var material = await _materialService.GetMaterial(id);
                return Ok(material);
            }
            catch (MaterialNotFoundException ex)
            {
                _logger.LogWarning(ex, "Material with Id: {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred while retrieving the material with Id: {Id}", id);
                return StatusCode(500, "An error ocurred while retrieving the material");
            }
        }
    }
}
