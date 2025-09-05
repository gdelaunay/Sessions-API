using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sessions_API.Data;
using Sessions_API.Models;
using Sessions_API.Services;

namespace Sessions_API.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class ForecastController(WeatherApiService weatherService, AppDbContext context, ILogger<ForecastController> logger) : ControllerBase 
{
      
    private readonly AppDbContext _context = context;

    [HttpGet("daily")]
    public async Task<ActionResult<List<Forecast>?>> GetDaily(float lat, float lon)
    {
        try
        {
            var dailyForecast = await weatherService.GetDailyForecast(lat, lon);
            return Ok(dailyForecast);
        }
        catch (HttpRequestException e)
        {
            logger.LogError("Échec de l'appel à l'API météo open-meteo.com : {MarineResponseReasonPhrase}", e.Message);
            return StatusCode(503, $"Échec de l'appel à l'API météo open-meteo.com : {e.Message}");
        }
    }
    
    [HttpGet("3hourly")]
    public async Task<ActionResult<List<Forecast>?>> Get3Hourly(float lat, float lon)
    {
        try
        {
            var hourly3Forecast = await weatherService.Get3HourlyForecast(lat, lon);
            return Ok(hourly3Forecast);
        }
        catch (HttpRequestException e)
        {
            logger.LogError("Échec de l'appel à l'API météo open-meteo.com : {WeatherResponseReasonPhrase}", e.Message);
            return StatusCode(503, $"Échec de l'appel à l'API météo open-meteo.com : {e.Message}");
        }
    }
  
    [HttpGet]
    public async Task<ActionResult<List<Forecast>>> GetAll()
    {
        return Ok(await _context.Forecasts.ToListAsync());
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Forecast>> Get(int id)
    {
        return await _context.Forecasts.FirstOrDefaultAsync(s => s.Id == id) ?? (ActionResult<Forecast>) NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<Forecast>> Post(Forecast forecast)
    {
        _context.Forecasts.Add(forecast);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = forecast.Id }, forecast);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Forecast>> Put(int id, Forecast updatedForecast)
    {
        var forecast = await _context.Forecasts.FindAsync(id);
        if (forecast == null)
        {
            return BadRequest("La prévision n'existe pas.");
        }
        _context.Entry(forecast).CurrentValues.SetValues(updatedForecast);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Forecast>> Delete(int id)
    {
        var forecast = await _context.Forecasts.FindAsync(id);
        if (forecast == null)
        {
            return NotFound("La prévision n'existe pas.");
        }
        _context.Forecasts.Remove(forecast);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
