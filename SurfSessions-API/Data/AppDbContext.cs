using Microsoft.EntityFrameworkCore;

namespace SurfSessions_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    
}
