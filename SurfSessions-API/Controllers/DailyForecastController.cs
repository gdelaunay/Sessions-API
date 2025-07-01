using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SurfSessions_API.Data;
using SurfSessions_API.DTOs;
using SurfSessions_API.Services;

namespace SurfSessions_API.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class DailyForecastController(WeatherApiService weatherService, ILogger<DailyForecastController> logger)
{
    public async Task<DailyForecast?> Get(float lat, float lon)
    {
        return await weatherService.GetDailyForecast(lat, lon);
    }

}
