namespace SurfSessions_API.Models;

public class Forecast(
    string dateTime,
    int weatherCode,
    float temperature,
    float waveHeight,
    int waveDirection,
    float wavePeriod,
    float windSpeed,
    int windDirection)
{
    public string DateTime { get; set; } = dateTime;
    public int WeatherCode { get; set; } = weatherCode;
    public float Temperature { get; set; } = temperature;
    public float WaveHeight { get; set; } = waveHeight;
    public int WaveDirection { get; set; } = waveDirection;
    public float WavePeriod { get; set; } = wavePeriod;
    public float WindSpeed { get; set; } = windSpeed;
    public int WindDirection { get; set; } = windDirection;
}
