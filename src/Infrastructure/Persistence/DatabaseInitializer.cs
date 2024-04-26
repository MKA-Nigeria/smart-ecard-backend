using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Initialization;
internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        await InitializeDbAsync(cancellationToken);
    }

    private async Task InitializeDbAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetPendingMigrations().Any())
        {
            _logger.LogInformation("Applying Root Migrations.");
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}