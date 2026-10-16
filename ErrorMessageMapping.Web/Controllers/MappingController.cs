using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using ErrorMessageMapping.Web.Pages;
using Microsoft.AspNetCore.Mvc;

namespace ErrorMessageMapping.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MappingController : ControllerBase
{
    private readonly ErrorMessageMappingService _service;
    private readonly ILogger<IndexModel> _logger;

    public MappingController(ErrorMessageMappingService service, ILogger<IndexModel> logger)
    {
        _service = service;
        _logger = logger;
    }

    public class ConvertRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("Convert API called with empty message");
            return BadRequest("Message is required.");
        }

        try
        {
            // throw new Exception("測試全域例外");
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ErrorMapping mapping)
    {
        if (string.IsNullOrWhiteSpace(mapping.MatchValue1))
            return BadRequest("MatchValue1 is required.");

        if (mapping.MatchType != MappingConstants.MatchTypeExact &&
            mapping.MatchType != MappingConstants.MatchTypeFuzzy)
            return BadRequest("MatchType must be Exact or Fuzzy.");

        await _service.AddMappingAsync(mapping);
        _logger.LogInformation("API Create called, MessageLength={Length}", mapping.MatchValue1?.Length ?? 0);
        return Ok(mapping);
    }
}