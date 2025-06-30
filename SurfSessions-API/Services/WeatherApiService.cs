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
        
        var daily = JsonDocument.Parse(json).RootElement.GetProperty("daily");

        var dates = new List<string>();
        foreach (JsonElement date in daily.GetProperty("time").EnumerateArray())
        {
            dates.Add(date.GetString() ?? string.Empty);
        }

        var heights = new List<float>();
        foreach (JsonElement height in daily.GetProperty("wave_height_max").EnumerateArray().ToList())
        {
            heights.Add(height.GetSingle());   
        }
        
        var directions =  new List<int>();
        foreach (JsonElement direction in daily.GetProperty("wave_direction_dominant").EnumerateArray().ToList())
        {
            directions.Add(direction.GetInt32());
        }
        
        var periods =  new List<float>();
        foreach (JsonElement period in daily.GetProperty("wave_period_max").EnumerateArray().ToList())
        {
            periods.Add(period.GetSingle()); 
        }
        
        return new DailyForecast(dates, heights, directions, periods);
    }
}
