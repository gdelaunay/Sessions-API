using Microsoft.EntityFrameworkCore;
using SurfSessions_API.Models;

namespace SurfSessions_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();
    
}
