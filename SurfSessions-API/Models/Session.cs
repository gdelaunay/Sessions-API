using System.ComponentModel.DataAnnotations;

namespace SurfSessions_API.Models;

public class Session
{ 
    public Session() {}
    
    public Session(Spot spot, Forecast forecast, DateTime startTime, DateTime endTime, int rating, string comment)
    {
        Spot = spot;
        Forecast = forecast;
        StartTime = startTime;
        EndTime = endTime;
        Rating = rating;
        Comment = comment;
    }
    
    public int Id { get; set; }
    public required  Spot Spot { get; set; }
    public required  Forecast Forecast { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? Rating { get; set; }
    [MaxLength(800)]
    public string Comment { get; set; } = string.Empty;
}
