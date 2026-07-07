var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL container (dev only). The resource/connection-string name is "Db"
// so it maps onto the app's existing ConnectionStrings:Db key. The actual
// database inside Postgres is "doodoodb".
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var db = postgres.AddDatabase("Db", "doodoodb");

// Migrator runs all module migrations once, then exits. The API waits for it.
var migrator = builder.AddProject<Projects.Doodoo_MigrationService>("migrator")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.DoodooApi>("api")
    .WithReference(db)
    .WaitForCompletion(migrator);

builder.Build().Run();
