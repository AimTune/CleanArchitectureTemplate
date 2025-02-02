using Application.Repositories.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories.Test;

namespace Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.ConnectionString));
        services.AddScoped<IUnitOfWork>(factory => factory.GetRequiredService<AppDbContext>());

        using IServiceScope scope = services.BuildServiceProvider().CreateScope();

        using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // dbContext.Database.MigrateAsync().Wait();

        services.AddTransient<ITestWriteRepository, TestWriteRepository>();
    }
}
