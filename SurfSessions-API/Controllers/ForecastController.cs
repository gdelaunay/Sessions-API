using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SurfSessions_API.Models;
using SurfSessions_API.Services;

namespace SurfSessions_API.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class ForecastController(WeatherApiService weatherService, ILogger<ForecastController> logger) : ControllerBase 
{
    [HttpGet("daily")]
    public async Task<ActionResult<List<Forecast>?>> GetDaily(float lat, float lon)
    {
        var dailyForecast = await weatherService.GetDailyForecast(lat, lon);
        if (dailyForecast == null)
        {
            return NotFound();
        }
        return Ok(dailyForecast);
    }
}
