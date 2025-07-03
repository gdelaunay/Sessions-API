using System.Text.Json;
using SurfSessions_API.Models;

namespace SurfSessions_API.Services;


public class WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger)
{
    private readonly string _marineApiUrl = "https://marine-api.open-meteo.com/v1";
    private readonly string _weatherApiUrl = "https://api.open-meteo.com/v1";

    public async Task<List<Forecast>?> GetDailyForecast(float lat, float lon)
    {
        const string marineDailyParams = "daily=wave_height_max,wave_direction_dominant,wave_period_max&timezone=Europe%2FBerlin";
        var marineDailyUrl = $"{_marineApiUrl}/marine?latitude={lat}&longitude={lon}&{marineDailyParams}";

        const string weatherDailyParams = "daily=weather_code,temperature_2m_max,wind_speed_10m_max,wind_direction_10m_dominant&timezone=Europe%2FBerlin&wind_speed_unit=kn";
        var weatherDailyUrl =  $"{_weatherApiUrl}/forecast?latitude={lat}&longitude={lon}&{weatherDailyParams}";
        
        // Appel API météo marine - Vagues
        logger.LogInformation($"HTTP request url : {marineDailyUrl}");
        var marineResponse = await httpClient.GetAsync(marineDailyUrl);
        if (!marineResponse.IsSuccessStatusCode)
        {
            logger.LogError($"Échec de l'appel à l'API météo open-meteo.com : {marineResponse.Content.ReadAsStringAsync()}");
            return null;
        }
        
        // Appel API météo marine - Vent et météo
        logger.LogInformation($"HTTP request url : {weatherDailyUrl}");
        var weatherResponse = await httpClient.GetAsync(weatherDailyUrl);
        if (!weatherResponse.IsSuccessStatusCode)
        {
            logger.LogError($"Échec de l'appel à l'API météo open-meteo.com : {weatherResponse.Content.ReadAsStringAsync()}");
            return null;
            
        }
        
        var marineDailyJson = JsonDocument.Parse(await marineResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("daily");
        var dates = marineDailyJson.GetProperty("time").EnumerateArray().ToList();
        var heights = marineDailyJson.GetProperty("wave_height_max").EnumerateArray().ToList();
        var directions = marineDailyJson.GetProperty("wave_direction_dominant").EnumerateArray().ToList();
        var periods =  marineDailyJson.GetProperty("wave_period_max").EnumerateArray().ToList();
        
        var weatherDailyJson = JsonDocument.Parse(await weatherResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("daily");
        var weatherCodes = weatherDailyJson.GetProperty("weather_code").EnumerateArray().ToList();
        var temperatures = weatherDailyJson.GetProperty("temperature_2m_max").EnumerateArray().ToList();
        var windSpeeds = weatherDailyJson.GetProperty("wind_speed_10m_max").EnumerateArray().ToList();
        var windDirections = weatherDailyJson.GetProperty("wind_direction_10m_dominant").EnumerateArray().ToList();
        
        var forecasts = new List<Forecast>();
        foreach (var (i, date) in dates.Index())
        {
            var fDate = dates[i].GetString() ?? string.Empty;
            var fHeight = heights[i].GetSingle();
            var fDirection = directions[i].GetInt32();
            var fPeriod = periods[i].GetSingle();
            var fWeatherCode = weatherCodes[i].GetInt32();
            var fTemperature = temperatures[i].GetSingle();
            var fWindSpeed = windSpeeds[i].GetSingle();
            var fWindDirection = windDirections[i].GetInt32();
            forecasts.Add(new Forecast(fDate, fWeatherCode, fTemperature, fHeight, fDirection, fPeriod, fWindSpeed, fWindDirection));
        }
        
        return forecasts;
    }
}
