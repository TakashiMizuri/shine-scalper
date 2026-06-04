using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;
using ShineScalper.Core.Services;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthDto> Get(
        [FromServices] TradingStateCache cache,
        [FromServices] IMarketDataStream marketData,
        [FromServices] IOptions<TradingOptions> trading) =>
        Ok(new HealthDto(
            "ok",
            trading.Value.Mode.ToString(),
            marketData.IsConnected,
            cache.LastPrice,
            trading.Value.Symbol));
}
