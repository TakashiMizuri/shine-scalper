using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LevelsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<LevelDto>> Get(
        [FromServices] ILevelDetector detector,
        [FromServices] IOptions<TradingOptions> trading,
        [FromQuery] string? symbol)
    {
        var sym = symbol ?? trading.Value.Symbol;
        return Ok(detector.GetActiveLevels(sym).Select(LevelDto.From));
    }
}
