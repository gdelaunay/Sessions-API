namespace SurfSessions_API.DTOs;

public class DailyForecast(
    List<string> dates,
    List<float> waveHeights,
    List<int> waveDirections,
    List<float> wavePeriods)
{
    public List<string> Dates { get; set; } = dates;
    public List<float> WaveHeights { get; set; } = waveHeights;
    public List<int> WaveDirections { get; set; } = waveDirections;
    public List<float> WavePeriods { get; set; } = wavePeriods;
}
