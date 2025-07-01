namespace SurfSessions_API.DTOs;

public class DailyForecast(
    List<string> dates,
    List<int> weatherCodes,
    List<float> temperatures,
    List<float> waveHeights,
    List<int> waveDirections,
    List<float> wavePeriods,
    List<float> windSpeeds,
    List<int> windDirections)
{
    public List<string> Dates { get; set; } = dates;
    public List<int> WeatherCodes { get; set; } = weatherCodes;
    public List<float> Temperatures { get; set; } = temperatures;
    public List<float> WaveHeights { get; set; } = waveHeights;
    public List<int> WaveDirections { get; set; } = waveDirections;
    public List<float> WavePeriods { get; set; } = wavePeriods;
    public List<float> WindSpeeds { get; set; } = windSpeeds;
    public List<int> WindDirections { get; set; } = windDirections;
}
