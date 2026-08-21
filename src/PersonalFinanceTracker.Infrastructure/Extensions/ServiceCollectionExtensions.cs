using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;
using PersonalFinanceTracker.Infrastructure.Data;
using PersonalFinanceTracker.Infrastructure.Repositories;
using PersonalFinanceTracker.Infrastructure.Services;

namespace PersonalFinanceTracker.Infrastructure.Extensions;

/// <summary>
/// Registers all Infrastructure services with the DI container.
/// Called from Program.cs to keep the API project thin.
///
/// Database provider is selected via the "DatabaseProvider" configuration key:
///   "Sqlite"    — default, cross-platform, no installation required (local dev)
///   "SqlServer" — for Azure SQL / production deployments
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core — provider selected from configuration ────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        var provider = configuration["DatabaseProvider"] ?? "Sqlite";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                // Azure / production: full SQL Server with migration history
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName));
            }
            else
            {
                // Default: SQLite — creates a single .db file, works on all OSes
                options.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IUserRepository,        UserRepository>();
        services.AddScoped<IAccountRepository,     AccountRepository>();
        services.AddScoped<ICategoryRepository,    CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository,      BudgetRepository>();

        // ── Services ──────────────────────────────────────────────────────────
        services.AddScoped<IAuthService,        AuthService>();
        services.AddScoped<IAccountService,     AccountService>();
        services.AddScoped<ICategoryService,    CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IBudgetService,      BudgetService>();
        services.AddScoped<IReportService,      ReportService>();

        return services;
    }
}
