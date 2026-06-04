using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShineScalper.Api.Dtos;
using ShineScalper.Core.Options;

namespace ShineScalper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ConfigController : ControllerBase
{
    [HttpGet]
    public ActionResult<ConfigDto> Get([FromServices] IOptions<TradingOptions> trading) =>
        Ok(ConfigDtoMapper.From(trading.Value));
}
