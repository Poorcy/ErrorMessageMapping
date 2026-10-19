using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ErrorMessageMapping.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MappingController : ControllerBase
{
    private readonly ErrorMessageMappingService _service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MappingController> _logger;

    public MappingController(
        ErrorMessageMappingService service,
        IConfiguration configuration,
        ILogger<MappingController> logger)
    {
        _service = service;
        _configuration = configuration;
        _logger = logger;
    }

    public class ConvertRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest request)
    {
        // API Key 檢查
        var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        var configuredKey = _configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(providedKey) || providedKey != configuredKey)
        {
            _logger.LogWarning("API Key validation failed");
            return Unauthorized("Invalid API Key");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("Convert API called with empty message");
            return BadRequest("Message is required.");
        }

        try
        {
            var result = await _service.ConvertAsync(request.Message);
            _logger.LogInformation("API Convert called, MessageLength={Length}, IsHit={IsHit}, MatchType={MatchType}",
                request.Message.Length, result.IsHit, result.MatchType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Convert API failed. MessageLength={Length}", request.Message?.Length ?? 0);
            return StatusCode(500, "Internal Server Error");
        }
    }

    //[HttpPost]
    //public async Task<IActionResult> Create([FromBody] ErrorMapping mapping)
    //{
    //    
    //    if (string.IsNullOrWhiteSpace(mapping.MatchValue1))
    //        return BadRequest("MatchValue1 is required.");

    //    if (mapping.MatchType != MappingConstants.MatchTypeExact &&
    //        mapping.MatchType != MappingConstants.MatchTypeFuzzy)
    //        return BadRequest("MatchType must be Exact or Fuzzy.");

    //    await _service.AddMappingAsync(mapping);
    //    _logger.LogInformation("API Create called, Value1={Value1}", mapping.MatchValue1);
    //    return Ok(mapping);
    //}
}