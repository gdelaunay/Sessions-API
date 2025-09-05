using System.ComponentModel.DataAnnotations;

namespace SurfSessions_API.Models;

public class Forecast
{
    public Forecast() {}

    public Forecast(string dateTime, int weatherCode, float temperature, float waveHeight, int waveDirection, float wavePeriod, float windSpeed, int windDirection)
    {
        DateTime = dateTime;
        WeatherCode = weatherCode;
        Temperature = temperature;
        WaveHeight = waveHeight;
        WaveDirection = waveDirection;
        WavePeriod = wavePeriod;
        WindSpeed = windSpeed;
        WindDirection = windDirection;
    }
    
    public int Id { get; set; } 
    
    [MaxLength(255)]
    public string DateTime { get; set; } = string.Empty;
    public int WeatherCode { get; set; }
    public float Temperature { get; set; }
    public float WaveHeight { get; set; }
    public int WaveDirection { get; set; }
    public float WavePeriod { get; set; }
    public float WindSpeed { get; set; }
    public int WindDirection { get; set; }
}