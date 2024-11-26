using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using react_native_app_bk.Models.TestSpecimenTypeModel;
using react_native_app_bk.Services;

namespace react_native_app_bk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSpecimenTypeController : ControllerBase
    {
        private readonly ITestSpecimenTypeService _testSpecimenTypeService;
        private readonly ILogger<TestSpecimenTypeController> _logger;

        public TestSpecimenTypeController(ITestSpecimenTypeService testSpecimenTypeService, ILogger<TestSpecimenTypeController> logger)
        {
            _testSpecimenTypeService = testSpecimenTypeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                IEnumerable<TestSpecimenType> testSpecimenTypes = await _testSpecimenTypeService.GetAllTestSpecimenTypes();
                return Ok(testSpecimenTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all test specimen types.");
                return StatusCode(500, "An error occurred while retrieving all test specimen types.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTestSpecimenType(int id)
        {
            try
            {
                var testSpecimenType = await _testSpecimenTypeService.GetTestSpecimenType(id);
                return Ok(testSpecimenType);
            }
            catch (TestSpecimenTypeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Test specimen type with Id: {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred while retrieving the test specimen type with Id: {Id}", id);
                return StatusCode(500, "An error ocurred while retrieving the test specimen type");
            }
        }
    }
}
