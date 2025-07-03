namespace SurfSessions_API.Models;

public class Session(Spot spot, Forecast forecast, DateTime startTime, DateTime endTime, int rating, string comment)
{
    public Spot Spot { get; set; } = spot;
    public Forecast Forecast { get; set; } = forecast;
    public DateTime StartTime { get; set; } = startTime;
    public DateTime EndTime { get; set; } = endTime;
    public int Rating { get; set; } = rating;
    public string Comment { get; set; } = comment;
}