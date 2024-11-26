using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using react_native_app_bk.Models.SampleModel;
using react_native_app_bk.Models.SampleModel.Dtos;
using react_native_app_bk.Services;

namespace react_native_app_bk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly ISampleService _sampleService;
        private readonly ILogger<SampleController> _logger;

        public SampleController(ISampleService sampleService, ILogger<SampleController> logger)
        {
            _sampleService = sampleService;
            _logger = logger;
        }

        // Action to get all samples
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                IEnumerable<Sample> samples = await _sampleService.GetAllSamples();
                return Ok(samples);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all samples.");
                return StatusCode(500, "An error occurred while retrieving all samples.");
            }
        }

        // Action to get a sample by Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSample(int id)
        {
            try
            {
                var sample = await _sampleService.GetSample(id);
                return Ok(sample);
            }
            catch (SampleNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sample with Id: {Id} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred while retrieving the sample with Id: {Id}", id);
                return StatusCode(500, "An error ocurred while retrieving the sample");
            }
        }

        // Action to create a new sample
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateSample([FromBody] SampleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sample = new Sample
                {
                    Sample_Number = model.Sample_Number,
                    Sample_Type_Id = model.Sample_Type_Id,
                    Material_Id = model.Material_Id,
                    Dimentions = model.Dimentions,
                    Test_Specimen_Type_Id = model.Test_Specimen_Type_Id,
                    Observations = model.Observations,
                    Date_Received = DateTime.Now
                };

                // Create the sample
                var (samplecreated, errorMessage) = await _sampleService.CreateSample(sample);

                // Check if the sample was created successfully
                if (!samplecreated)
                {
                    _logger.LogWarning("Error creating the sample: {ErrorMessage}", errorMessage);
                    return BadRequest(errorMessage);
                }

                return Ok("Sample successfully created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred while creating the sample.");
                return StatusCode(500, "An error ocurred while creating the sample.");
            }
        }
    }
}
