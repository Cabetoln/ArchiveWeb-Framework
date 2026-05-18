using Archive.API.DTOs;
using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/seasonal-analysis")]
[Produces("application/json")]
public class SeasonalAnalysisController(ISeasonalAnalysisService seasonalAnalysisService) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType(typeof(SeasonalAnalysisStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending()
    {
        var status = await seasonalAnalysisService.GetPendingStatusAsync();
        return Ok(status);
    }

    [HttpPost("process")]
    [Authorize]
    [ProducesResponseType(typeof(SeasonalAnalysisStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessPending()
    {
        var status = await seasonalAnalysisService.ProcessPendingAnalysisAsync();
        return Ok(status);
    }
}
