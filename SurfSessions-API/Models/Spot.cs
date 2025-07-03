namespace SurfSessions_API.Models;

public class Spot(string name, float latitude, float longitude, List<Session> sessions)
{
    public string Name { get; set; } = name;
    public float Latitude { get; set; } = latitude;
    public float Longitude { get; set; } = longitude;
    public List<Session> Sessions { get; set; } = sessions;
}