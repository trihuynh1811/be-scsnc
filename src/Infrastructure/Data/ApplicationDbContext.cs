using System.Reflection;
using Application.Common.Interfaces;
using BCrypt.Net;
using Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("SCSnC_DB"));
    }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<User>().HasData(
            new User { UserId = 1, Username = "mah ballz", Email = "1@gmail.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Role = Infrastructure.Enum.Role.Admin },
            new User { UserId = 2, Username = "mai ni gà", Email = "2@gmail.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Role = Infrastructure.Enum.Role.User },
            new User { UserId = 3, Username = "big schlong", Email = "3@gmail.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Role = Infrastructure.Enum.Role.None }
        );
    }
}