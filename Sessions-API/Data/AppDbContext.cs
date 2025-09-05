using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sessions_API.Models;

namespace Sessions_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();
    
}
