using System.ComponentModel.DataAnnotations;

namespace SurfSessions_API.Models;

public class Spot
{
    public Spot() { Sessions = new List<Session>(); }

    public Spot(string name, float latitude, float longitude, List<Session>? sessions = null)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        Sessions = sessions ?? [];
    }
    
    public int Id { get; set; } 
    [MaxLength(255)]
    public required string Name { get; set; } = string.Empty;
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public List<Session> Sessions { get; set; }
}
