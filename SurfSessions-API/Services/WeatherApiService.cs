using System.Text.Json;
using SurfSessions_API.DTOs;

namespace SurfSessions_API.Services;


public class WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger)
{
    private readonly string _weatherApiUrl = "https://marine-api.open-meteo.com/v1";

    public async Task<DailyForecast?> GetDailyForecast(float lat, float lon)
    {
        const string marineDailyParams = "daily=wave_height_max,wave_direction_dominant,wave_period_max&timezone=Europe%2FBerlin";
        var marineDailyUrl = $"{_weatherApiUrl}/marine?latitude={lat}&longitude={lon}&{marineDailyParams}";

        const string weatherDailyParams = "daily=weather_code,temperature_2m_max,wind_speed_10m_max,wind_direction_10m_dominant&timezone=Europe%2FBerlin&wind_speed_unit=kn";
        var weatherDailyUrl =  $"{_weatherApiUrl}/forecast?latitude={lat}&longitude={lon}&{weatherDailyParams}";
        
        // Appel API météo marine - Vagues
        var response = await httpClient.GetAsync(marineDailyUrl);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(await response.Content.ReadAsStringAsync());
        }
        var json = await response.Content.ReadAsStringAsync();
        
        var daily = JsonDocument.Parse(json).RootElement.GetProperty("daily");

        var dates = new List<string>();
        foreach (var date in daily.GetProperty("time").EnumerateArray())
        {
            dates.Add(date.GetString() ?? string.Empty);
        }

        var heights = new List<float>();
        foreach (var height in daily.GetProperty("wave_height_max").EnumerateArray())
        {
            heights.Add(height.GetSingle());   
        }
        
        var directions =  new List<int>();
        foreach (var direction in daily.GetProperty("wave_direction_dominant").EnumerateArray())
        {
            directions.Add(direction.GetInt32());
        }
        
        var periods =  new List<float>();
        foreach (var period in daily.GetProperty("wave_period_max").EnumerateArray())
        {
            periods.Add(period.GetSingle()); 
        }
                
        // Appel API météo marine - Vent et météo
        response = await httpClient.GetAsync(weatherDailyUrl);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(await response.Content.ReadAsStringAsync());
        }
        json = await response.Content.ReadAsStringAsync();
        
        daily = JsonDocument.Parse(json).RootElement.GetProperty("daily");
        
        var weatherCodes = new List<int>();
        foreach (var weatherCode in daily.GetProperty("weather_code").EnumerateArray())
        {
            weatherCodes.Add(weatherCode.GetInt32());
        }

        var temperatures = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(temperature => temperature.GetSingle()).ToList();
        var windSpeeds = daily.GetProperty("wind_speed_10m_max").EnumerateArray().Select(windSpeed => windSpeed.GetSingle()).ToList();

        var windDirections = new List<int>();
        foreach (var windDirection in daily.GetProperty("wind_direction_10m_dominant").EnumerateArray())
        {
            windDirections.Add(windDirection.GetInt32());
        }

        return new DailyForecast(dates, 
            weatherCodes,
            temperatures,
            heights, 
            directions, 
            periods,
            windSpeeds,
            windDirections);
    }
}
