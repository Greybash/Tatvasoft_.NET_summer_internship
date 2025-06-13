using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Movies.DataAccess;
using Movies.Services;
using Movies.Helpers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Option 1: Ignore cycles (recommended for most cases)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

       

        // Additional settings for better JSON handling
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });


// Database - Using PostgreSQL
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>(); // Added UserService
builder.Services.AddScoped<UserMovieService>();
builder.Services.AddScoped<MissionThemeService>(); // Added MissionThemeService
builder.Services.AddScoped<MissionSkillService>(); // Added MissionSkillService
builder.Services.AddScoped<MissionService>(); // Added MissionService
builder.Services.AddScoped<LocationService>(); // Added LocationService for Country/City
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<MissionApplicationService>();
builder.Services.AddScoped<UserDetailService>();

// Add CORS policy to allow Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Movies API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured");
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
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization with role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Add this BEFORE app.Run()
var currentDirectory = Directory.GetCurrentDirectory();
var uploadsPath = Path.Combine(currentDirectory, "UploadedImages");
var missionsPath = Path.Combine(uploadsPath, "MissionImages");

Console.WriteLine($"Current Directory: {currentDirectory}");
Console.WriteLine($"Uploads Path: {uploadsPath}");
Console.WriteLine($"Missions Path: {missionsPath}");
Console.WriteLine($"UploadedImages exists: {Directory.Exists(uploadsPath)}");
Console.WriteLine($"MissionImages exists: {Directory.Exists(missionsPath)}");
if (Directory.Exists(missionsPath))
{
    var files = Directory.GetFiles(missionsPath);
    Console.WriteLine($"Files found: {files.Length}");
    foreach (var file in files.Take(3))
    {
        Console.WriteLine($"  - {Path.GetFileName(file)}");
    }
}
else
{
    Console.WriteLine("MissionImages directory NOT FOUND!");
}

// Default static files
app.UseStaticFiles();

// Custom static files - MUST be before UseRouting()
// Remove duplicate calls - keep only one of each
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages")),
    RequestPath = "/UploadedImages"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/UploadedImages"
});

app.UseRouting(); // Keep only ONE UseRouting call
app.UseCors("AllowAngularDev"); // Keep only ONE UseCors call
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded with roles
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
    context.Database.EnsureCreated();

    // Seed roles if they don't exist
    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Movies.Models.Role { Name = "Admin", Description = "Administrator with full access", IsActive = true },
            new Movies.Models.Role { Name = "User", Description = "Regular user with limited access", IsActive = true }
        );
        context.SaveChanges();
    }
}

app.Run();