using Microsoft.AspNetCore.Mvc;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Interfaces;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PositionsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> Get(
        [FromServices] ITradeRepository repo,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var open = await repo.GetOpenPositionsAsync(ct);
        var recent = await repo.GetRecentPositionsAsync(limit, ct);
        return Ok(new
        {
            open = open.Select(PositionDto.From),
            recent = recent.Select(PositionDto.From)
        });
    }
}
