using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class HostExtensions
{
    public static void MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder)
        where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();

        try
        {
            context?.Database.Migrate();
            logger.LogInformation("Migrated successfully");
            seeder(context!, services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while migrating database!");
        }
    }
}