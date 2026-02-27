using IvaFlashSaleEngine.Middleware;
using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DYNAMIC PORT BINDING (Essential for Render)
// Render assigns a random port via the 'PORT' environment variable.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// STRUCTURED LOGGING
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
   // .WriteTo.File("logs/engine_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// CORE API SERVICES
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT SECURITY CONFIGURATION
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("CRITICAL: JWT Key not found. Check User Secrets or Environment Variables.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Removes 5min grace period for high-volume sales
        };
    });

builder.Services.AddAuthorization();

// HYBRID DB PROVIDER (Local SQL Server vs Cloud Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Use SQL Server for our local dev environment
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Use PostgreSQL for Production (Supabase)
        // Ensure our connection string includes: "SSL Mode=Require;Trust Server Certificate=true;"
        options.UseNpgsql(connectionString);
    }
});

// DEPENDENCY INJECTION
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// AUTOMATED CLOUD SETUP (Migrations & Seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Auto-run migrations in Production to build Supabase tables
        if (!app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }

        // Seed the initial Admin from AppSettings/EnvVars
        await DbInitializer.SeedAdminAsync(services, builder.Configuration);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize Database during startup.");
    }
}

// MIDDLEWARE PIPELINE
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Always first!

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// STARTUP LOGIC
try
{
    Log.Information("IvaFlashSale starting in {Env} mode on port {Port}", app.Environment.EnvironmentName, port);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}