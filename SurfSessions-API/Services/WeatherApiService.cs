using System.Text.Json;
using SurfSessions_API.DTOs;

namespace SurfSessions_API.Services;


public class WeatherApiService(HttpClient httpClient)
{
    public async Task<DailyForecast?> GetDailyForecast()
    {
        var response = await httpClient.GetAsync("https://marine-api.open-meteo.com/v1/marine?latitude=54.544587&longitude=10.227487&daily=wave_height_max,wave_direction_dominant,wave_period_max&timezone=Europe%2FBerlin");
        response.EnsureSuccessStatusCode();
        Console.WriteLine("response : " + await response.Content.ReadAsStringAsync());
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("json: " + json);
        return JsonSerializer.Deserialize<DailyForecast>(json);
    }
}
