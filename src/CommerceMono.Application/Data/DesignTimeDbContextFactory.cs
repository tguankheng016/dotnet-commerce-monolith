using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceMono.Application.Data;

// Used For dotnet ef add or update migration only
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        builder.UseNpgsql("Server=localhost;Port=5433;Database=CommerceMonoDb;User Id=postgres;Password=myStong_Password123#;Include Error Detail=true")
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(builder.Options);
    }
}
