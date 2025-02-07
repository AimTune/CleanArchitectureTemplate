using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.DbContexts;

public interface IApplicationDbContext
{
    // EF Core Entities-DbSets

    // EF Core Core Modules
    public DatabaseFacade Database { get; }
    public DbSet<TEntity> Set<TEntity>() where TEntity : class;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
