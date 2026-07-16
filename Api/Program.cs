using Doodoo.Modules.Currency;
using Doodoo.Modules.Inventory;
using Doodoo.Modules.Rewards;
using Doodoo.Modules.Todos;
using Doodoo.Modules.Users;
using DoodooApi.Models.Main.Users;
using DoodooApi.Services;
using DoodooApi.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using JasperFx.CodeGeneration.Model;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults: OpenTelemetry, health checks, service discovery, resilience.
builder.AddServiceDefaults();

// Wolverine is used ONLY for in-process communication between modules.
// No external transports are configured (no RabbitMQ/Azure/etc.).
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Doodoo.Modules.Currency.Anchor).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Doodoo.Modules.Rewards.Anchor).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Doodoo.Modules.Todos.Anchor).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Doodoo.Modules.Users.Anchor).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Doodoo.Modules.Inventory.Anchor).Assembly);

    // Module DbContexts are registered via AddDbContext, whose DbContextOptions is an
    // opaque scoped factory Wolverine cannot inline. Allow the generated handler code to
    // resolve those services from the container (service location) instead.
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
});

// Controllers live in the host and in the modules that own them; register each module
// assembly as an application part so its controllers are discovered.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Doodoo.Modules.Inventory.Anchor).Assembly);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PublicApi", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DoodooApi", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Geef alleen de AccessToken"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    // Show enum name<->value mappings in the schema descriptions.
    options.SchemaFilter<EnumSchemaFilter>();
});
builder.Services.AddHttpContextAccessor();

// Dependency Injection
builder.Services.AddScoped<UserService>();
// Expose the host's current-user resolution to module controllers via the shared abstraction.
builder.Services.AddScoped<Doodoo.SharedKernel.Abstractions.ICurrentUser>(
    sp => sp.GetRequiredService<UserService>());

var connectionString = builder.Configuration.GetConnectionString("Db");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'Db' is not defined.");
}

// Modules
builder.Services.AddUsersModule(connectionString);
builder.Services.AddCurrencyModule(connectionString);
builder.Services.AddRewardsModule(connectionString);
builder.Services.AddTodosModule(connectionString);
builder.Services.AddInventoryModule(connectionString);

builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;

    opt.Password.RequiredLength = 8;
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireNonAlphanumeric = true;
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<UsersDbContext>()
    .AddApiEndpoints()
    .AddUserManager<AppUserManager>();

builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("PublicApi");
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<AppUser>();

app.MapControllers();

// Aspire default endpoints (/health, /alive in Development).
app.MapDefaultEndpoints();

await EnsureAdminRoleAsync(app);

app.Run();

// Ensure the "Admin" role exists and assign it to any configured admin emails
// (config key "Admin:Emails" — array or comma-separated). Idempotent.
static async Task EnsureAdminRoleAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));

    var configured = app.Configuration.GetSection("Admin:Emails").Get<string[]>()
        ?? (app.Configuration["Admin:Emails"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    if (configured is null) return;

    var userManager = sp.GetRequiredService<UserManager<AppUser>>();
    foreach (var email in configured)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");
    }
}
