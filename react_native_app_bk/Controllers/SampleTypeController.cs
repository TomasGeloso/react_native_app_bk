using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using react_native_app_bk.Models.SampleTypeModel;
using react_native_app_bk.Services;

namespace react_native_app_bk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleTypeController : ControllerBase
    {
        private readonly ISampleTypeService _sampleTypeService;
        private readonly ILogger<SampleTypeController> _logger;

        public SampleTypeController(ISampleTypeService sampleTypeService, ILogger<SampleTypeController> logger)
        {
            _sampleTypeService = sampleTypeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                IEnumerable<SampleType> sampleTypes = await _sampleTypeService.GetAllSampleTypes();
                return Ok(sampleTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all sample types.");
                return StatusCode(500, "An error occurred while retrieving all sample types.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSampleType(int id)
        {
            try
            {
                var sampleType = await _sampleTypeService.GetSampleType(id);
                return Ok(sampleType);
            }
            catch (SampleTypeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sample type with Id: {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred while retrieving the sample type with Id: {Id}", id);
                return StatusCode(500, "An error ocurred while retrieving the sample type");
            }
        }
    }
}
