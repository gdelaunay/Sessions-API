using System.ComponentModel.DataAnnotations.Schema;

namespace SurfSessions_API.DTOs;

public class DailyForecast
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double generationtime_ms { get; set; }
    public int utc_offset_seconds { get; set; }
    public string timezone { get; set; }
    public string timezone_abbreviation { get; set; }
    public double elevation { get; set; }
    public DailyUnits daily_units { get; set; }
    public Daily daily { get; set; }
}

public class DailyUnits
{
    public string time { get; set; }
    public string wave_height_max { get; set; }
    public string wave_direction_dominant { get; set; }
    public string wave_period_max { get; set; }
}

public class Daily
{
    public List<string> time { get; set; }
    public List<double> wave_height_max { get; set; }
    public List<int> wave_direction_dominant { get; set; }
    public List<double> wave_period_max { get; set; }
}
