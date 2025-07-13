using Microsoft.EntityFrameworkCore;

namespace SurfSessions_API.Data;

public class AppDbService
{
    public static void MigrateIfNeeded(IServiceProvider services, int maxRetries = 3, int delaySeconds = 5)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AppDbService");
        var db = services.GetRequiredService<AppDbContext>();

        logger.LogInformation("Checking if there are pending database migrations ...");
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var pendingMigrations = db.Database.GetPendingMigrations();
                if (!pendingMigrations.Any())
                {
                    logger.LogInformation("Database is up-to-date. No migration needed.");
                    return;
                }

                logger.LogInformation("Pending migrations detected. Applying migrations ...");
                db.Database.Migrate();
                logger.LogInformation("Migrations applied successfully.");
                return;
            }
            catch (Exception ex)
            {
                if (attempt != maxRetries)
                {
                    logger.LogError("Database connection attempt {Attempt}/{maxRetries} failed ...", attempt, maxRetries);
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    logger.LogError("All {maxRetries} attempts to connect to the database have failed. Please ensure the database is " +
                                    "running and that the connection string at Program.cs:33 is correct. \n",  maxRetries);
                    throw;
                }
            }
        }
    }
}