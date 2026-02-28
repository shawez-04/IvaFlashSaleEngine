using IvaFlashSaleEngine.Data;
using IvaFlashSaleEngine.Infrastructure;
using IvaFlashSaleEngine.Middleware;
using IvaFlashSaleEngine.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////////////////////////
// PORT (Render Compatible)
//////////////////////////////////////////////////////////////
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

//////////////////////////////////////////////////////////////
// LOGGING
//////////////////////////////////////////////////////////////
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

//////////////////////////////////////////////////////////////
// CORE SERVICES
//////////////////////////////////////////////////////////////
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IvaFlashSale API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    options.OperationFilter<IdempotencyHeaderFilter>();

});

//////////////////////////////////////////////////////////////
// JWT CONFIGURATION
//////////////////////////////////////////////////////////////
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key missing.");
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

//////////////////////////////////////////////////////////////
// DATABASE
//////////////////////////////////////////////////////////////
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

//////////////////////////////////////////////////////////////
// DEPENDENCY INJECTION
//////////////////////////////////////////////////////////////
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//////////////////////////////////////////////////////////////
// CORS
//////////////////////////////////////////////////////////////
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

//////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////
var app = builder.Build();

//////////////////////////////////////////////////////////////
// DB INIT
//////////////////////////////////////////////////////////////
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (!app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }
        await DbInitializer.SeedAdminAsync(
            services,
            builder.Configuration
        );
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database initialization failed.");
    }
}

//////////////////////////////////////////////////////////////
// PIPELINE
//////////////////////////////////////////////////////////////
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

//////////////////////////////////////////////////////////////
// START
//////////////////////////////////////////////////////////////
Log.Information("IvaFlashSale running on port {Port}", port);
app.Run();