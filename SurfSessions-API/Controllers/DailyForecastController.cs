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
public class DailyForecastController(WeatherApiService weatherService)//AppDbContext context)
{
    //private readonly AppDbContext _context = context;
    private readonly WeatherApiService _weatherService = weatherService;

    [HttpGet]
    public async Task<DailyForecast?> Get()
    {
        var data = await _weatherService.GetDailyForecast();
        Console.WriteLine("data : " + JsonSerializer.Serialize(data));
        return data;
    }

}
