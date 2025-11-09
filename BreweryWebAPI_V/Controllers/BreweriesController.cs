using BreweryWebAPI_V.Exceptions;
using BreweryWebAPI_V.Services;
using Microsoft.AspNetCore.Mvc;

namespace BreweryWebAPI_V.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BreweriesController : ControllerBase
    {
        private readonly IBreweryService _service;
        private readonly ILogger<BreweriesController> _logger;

        public BreweriesController(IBreweryService service, ILogger<BreweriesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get breweries with optional search, sort, paging and distance calculation.
        /// Query: q (search), sort=name|city|distance, dir=asc|desc, lat, lon, page, pageSize
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> Get(
            [FromQuery] string? q,
            [FromQuery] string? sort,
            [FromQuery] string? dir = "asc",
            [FromQuery] double? lat = null,
            [FromQuery] double? lon = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            try
            {
                var results = await _service.GetBreweriesAsync(q, sort, dir, lat, lon, page, pageSize, ct);
                return Ok(results);
            }
            catch (ApiException ex)
            {
                _logger.LogWarning(ex, "Bad request");
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Upstream error");
                return StatusCode(503, new { message = "Upstream service error", detail = ex.Message });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Client cancelled request" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Autocomplete brewery names by prefix.
        /// </summary>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string prefix = "", [FromQuery] int limit = 10, CancellationToken ct = default)
        {
            var results = await _service.GetAutocompleteAsync(prefix, limit, ct);
            return Ok(results);
        }
    }
}
