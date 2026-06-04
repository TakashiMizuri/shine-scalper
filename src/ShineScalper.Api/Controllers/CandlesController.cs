using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CandlesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CandleDto>>> Get(
        [FromServices] ICandleRepository repo,
        [FromServices] IOptions<TradingOptions> trading,
        [FromQuery] string? symbol,
        [FromQuery] string interval = "5",
        [FromQuery] int limit = 500,
        CancellationToken ct = default)
    {
        var sym = symbol ?? trading.Value.Symbol;
        var candles = await repo.GetAsync(sym, interval, null, limit, ct);
        return Ok(candles.Select(CandleDto.From));
    }
}
