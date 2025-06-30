using System.Text.Json;
using SurfSessions_API.DTOs;

namespace SurfSessions_API.Services;


public class WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger)
{
    private readonly string _weatherApiUrl = "https://marine-api.open-meteo.com/v1";

    public async Task<DailyForecast?> GetDailyForecast(float lat, float lon)
    {
        var marineDailyParams = "daily=wave_height_max,wave_direction_dominant,wave_period_max&timezone=Europe%2FBerlin";
        var marineDailyUrl = $"{_weatherApiUrl}/marine?latitude={lat}&longitude={lon}&{marineDailyParams}";

        var weatherDailyParams = "";
        var weaterDailyUrl =  $"{_weatherApiUrl}/marine?latitude={lat}&longitude={lon}&{weatherDailyParams}";
        
        var response = await httpClient.GetAsync(marineDailyUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(await response.Content.ReadAsStringAsync());
        }
        
        var json = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine("json: " + json);
        
        return JsonSerializer.Deserialize<DailyForecast>(json);
    }
}
