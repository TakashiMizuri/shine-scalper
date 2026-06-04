using Microsoft.AspNetCore.Mvc;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Interfaces;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TradesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FillDto>>> Get(
        [FromServices] ITradeRepository repo,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var fills = await repo.GetFillsAsync(limit, ct);
        return Ok(fills.Select(FillDto.From));
    }
}
