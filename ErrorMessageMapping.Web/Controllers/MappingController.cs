using ErrorMessageMapping.Application;
using Microsoft.AspNetCore.Mvc;

namespace ErrorMessageMapping.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MappingController : ControllerBase
{
    private readonly ErrorMessageMappingService _service;

    public MappingController(ErrorMessageMappingService service)
    {
        _service = service;
    }

    public class ConvertRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var result = await _service.ConvertAsync(request.Message);
        return Ok(result);
    }
}