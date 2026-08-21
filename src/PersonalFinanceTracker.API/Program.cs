using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PersonalFinanceTracker.API.Middleware;
using PersonalFinanceTracker.Core.Mappings;
using PersonalFinanceTracker.Infrastructure.Data;
using PersonalFinanceTracker.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (EF Core, Repositories, Services) ──────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey    = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PersonalFinanceTracker";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtIssuer,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero  // no grace period on expiry
    };
});

// ── Authorization ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization();

// ── Controllers + JSON ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Personal Finance Tracker API",
        Version     = "v1",
        Description = "RESTful API for managing personal finances — accounts, transactions, budgets, and reports."
    });

    // JWT Bearer button in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS (allows React frontend on localhost:5173 / 3000) ─────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// ── Database initialisation ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsDevelopment())
    {
        // EnsureCreated() checks File.Exists() for SQLite — if the .db file
        // already exists (even empty, from a prior failed run) it returns false
        // and creates NO tables. Detect that case and force a clean rebuild.
        var schemaCreated = db.Database.EnsureCreated();

        if (!schemaCreated)
        {
            try
            {
                // Verify the schema is actually present
                _ = db.Users.Count();
            }
            catch
            {
                // File exists but tables are missing — wipe and rebuild
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
    }
    else
    {
        // Production (Azure SQL): apply pending EF migrations at startup.
        // Requires migration files generated with: dotnet ef migrations add
        db.Database.Migrate();
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Finance Tracker v1");
        c.RoutePrefix = "swagger"; // Swagger UI at: http://localhost:PORT/swagger
    });
}

// Skip HTTPS redirect in Development — avoids macOS dev-cert issues on localhost
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for integration tests
public partial class Program { }
