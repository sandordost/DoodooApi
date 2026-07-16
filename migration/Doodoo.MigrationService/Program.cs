using Doodoo.Modules.Currency;
using Doodoo.Modules.Inventory;
using Doodoo.Modules.Rewards;
using Doodoo.Modules.Todos;
using Doodoo.Modules.Users;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Db")
    ?? throw new InvalidOperationException("Connection string 'Db' is not defined.");

// One DbContext per module, all against the same Postgres database but each with its
// own schema and its own migration history table.
static Action<Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder> History(string schema)
    => npg =>
    {
        npg.MigrationsHistoryTable("__EFMigrationsHistory", schema);
        npg.EnableRetryOnFailure();
    };

builder.Services.AddDbContext<UsersDbContext>(o => o.UseNpgsql(connectionString, History(UsersDbContext.Schema)));
builder.Services.AddDbContext<CurrencyDbContext>(o => o.UseNpgsql(connectionString, History(CurrencyDbContext.Schema)));
builder.Services.AddDbContext<RewardsDbContext>(o => o.UseNpgsql(connectionString, History(RewardsDbContext.Schema)));
builder.Services.AddDbContext<TodosDbContext>(o => o.UseNpgsql(connectionString, History(TodosDbContext.Schema)));
builder.Services.AddDbContext<InventoryDbContext>(o => o.UseNpgsql(connectionString, History(InventoryDbContext.Schema)));

var host = builder.Build();

// Migrate sequentially in a fixed order (Postgres DDL takes advisory locks; running the
// contexts in parallel invites contention).
using (var scope = host.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await sp.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<CurrencyDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<RewardsDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<TodosDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
}

// Run-once: migrate and exit 0 so Aspire's WaitForCompletion gate opens.
