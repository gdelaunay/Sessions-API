namespace SurfSessions_API.DTOs;

public class DailyForecast
{
    public DailyForecast(List<string> dates, List<float> waveHeights, List<int> waveDirections, List<float> wavePeriods)
    {
        Dates = dates;
        WaveHeights = waveHeights;
        WaveDirections = waveDirections;
        WavePeriods = wavePeriods;
    }

    public List<string> Dates { get; set; }
    public List<float> WaveHeights { get; set; }
    public List<int> WaveDirections { get; set; }
    public List<float> WavePeriods { get; set; }
}
