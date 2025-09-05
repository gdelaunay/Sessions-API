using System.Text.Json;
using Sessions_API.Models;

namespace Sessions_API.Services;


public class WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger)
{
    private const string MarineApiUrl = "https://marine-api.open-meteo.com/v1";
    private const string MarineDailyParams = "daily=wave_height_max,wave_direction_dominant,wave_period_max&timezone=Europe%2FBerlin";
    private const string Marine3HourlyParams = "hourly=wave_height,wave_direction,wave_period&timezone=Europe%2FBerlin&temporal_resolution=hourly_3";
    
    private const string WeatherApiUrl = "https://api.open-meteo.com/v1";
    private const string WeatherDailyParams = "daily=weather_code,temperature_2m_max,wind_speed_10m_max,wind_direction_10m_dominant&timezone=Europe%2FBerlin&wind_speed_unit=kn";
    private const string Weather3HourlyParams = "hourly=weather_code,temperature_2m,wind_speed_10m,wind_direction_10m&timezone=Europe%2FBerlin&temporal_resolution=hourly_3&wind_speed_unit=kn";

    public async Task<List<Forecast>?> GetDailyForecast(float lat, float lon)
    {
        // Construction des urls
        var marineDailyUrl = $"{MarineApiUrl}/marine?latitude={lat}&longitude={lon}&{MarineDailyParams}";
        var weatherDailyUrl =  $"{WeatherApiUrl}/forecast?latitude={lat}&longitude={lon}&{WeatherDailyParams}";
        
        // Appel API météo marine - Vagues
        logger.LogInformation($"HTTP request url : {marineDailyUrl}");
        var marineResponse = await httpClient.GetAsync(marineDailyUrl);
        marineResponse.EnsureSuccessStatusCode(); 
        
        // Appel API météo marine - Vent et météo
        logger.LogInformation($"HTTP request url : {weatherDailyUrl}");
        var weatherResponse = await httpClient.GetAsync(weatherDailyUrl);
        weatherResponse.EnsureSuccessStatusCode();
        
        var marineJson = JsonDocument.Parse(await marineResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("daily");
        var weatherJson = JsonDocument.Parse(await weatherResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("daily");
        
        return ParseDailyForecast(marineJson, weatherJson);;
    }
    
    public async Task<List<Forecast>?> Get3HourlyForecast(float lat, float lon)
    {
        // Construction des urls
        var marine3HourlyUrl = $"{MarineApiUrl}/marine?latitude={lat}&longitude={lon}&{Marine3HourlyParams}";
        var weather3HourlyUrl =  $"{WeatherApiUrl}/forecast?latitude={lat}&longitude={lon}&{Weather3HourlyParams}";
        
        // Appel API météo marine - Vagues
        logger.LogInformation($"HTTP request url : {marine3HourlyUrl}");
        var marineResponse = await httpClient.GetAsync(marine3HourlyUrl);
        marineResponse.EnsureSuccessStatusCode(); 
        
        // Appel API météo marine - Vent et météo
        logger.LogInformation($"HTTP request url : {weather3HourlyUrl}");
        var weatherResponse = await httpClient.GetAsync(weather3HourlyUrl);
        weatherResponse.EnsureSuccessStatusCode();
        
        var marineJson = JsonDocument.Parse(await marineResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("hourly");
        var weatherJson = JsonDocument.Parse(await weatherResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("hourly");

        return Parse3HourlyForecast(marineJson, weatherJson);
    }
    
    private static List<Forecast> ParseDailyForecast(JsonElement marineJson, JsonElement weatherJson)
    {
        var dates = marineJson.GetProperty("time").EnumerateArray().ToList();
        var heights = marineJson.GetProperty("wave_height_max").EnumerateArray().ToList();
        var directions = marineJson.GetProperty("wave_direction_dominant").EnumerateArray().ToList();
        var periods =  marineJson.GetProperty("wave_period_max").EnumerateArray().ToList();
        var weatherCodes = weatherJson.GetProperty("weather_code").EnumerateArray().ToList();
        var temperatures = weatherJson.GetProperty("temperature_2m_max").EnumerateArray().ToList();
        var windSpeeds = weatherJson.GetProperty("wind_speed_10m_max").EnumerateArray().ToList();
        var windDirections = weatherJson.GetProperty("wind_direction_10m_dominant").EnumerateArray().ToList();
        
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
    
    private static List<Forecast> Parse3HourlyForecast(JsonElement marineJson, JsonElement weatherJson)
    {
        var dates = marineJson.GetProperty("time").EnumerateArray().ToList();
        var heights = marineJson.GetProperty("wave_height").EnumerateArray().ToList();
        var directions = marineJson.GetProperty("wave_direction").EnumerateArray().ToList();
        var periods =  marineJson.GetProperty("wave_period").EnumerateArray().ToList();
        var weatherCodes = weatherJson.GetProperty("weather_code").EnumerateArray().ToList();
        var temperatures = weatherJson.GetProperty("temperature_2m").EnumerateArray().ToList();
        var windSpeeds = weatherJson.GetProperty("wind_speed_10m").EnumerateArray().ToList();
        var windDirections = weatherJson.GetProperty("wind_direction_10m").EnumerateArray().ToList();
        
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
