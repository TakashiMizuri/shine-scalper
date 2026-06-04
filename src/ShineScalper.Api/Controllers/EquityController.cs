using Microsoft.AspNetCore.Mvc;
using ShineScalper.Core.Interfaces;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EquityController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> Get(
        [FromServices] ITradeRepository repo,
        [FromQuery] int limit = 200,
        CancellationToken ct = default)
    {
        var current = await repo.GetEquityAsync(ct);
        var history = await repo.GetEquityHistoryAsync(limit, ct);
        return Ok(new
        {
            current,
            history = history.Select(h => new { time = h.Time, equity = h.Equity })
        });
    }
}
