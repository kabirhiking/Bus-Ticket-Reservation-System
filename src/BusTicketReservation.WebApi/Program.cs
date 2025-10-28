using FluentValidation;
using BusTicketReservation.Application.Extensions;
using BusTicketReservation.Infrastructure.Extensions;
using BusTicketReservation.WebApi.Services;
using BusTicketReservation.WebApi.Validators;
using BusTicketReservation.WebApi.DTOs.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Supabase;
using Microsoft.EntityFrameworkCore;
using BusTicketReservation.Infrastructure.Data;
using BusTicketReservation.Application.Interfaces;

try
{
    Console.WriteLine("Starting application...");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddControllers();

// Add Application layer services
builder.Services.AddApplication();

// Add Supabase-based repositories for authentication BEFORE Infrastructure
// This prevents Infrastructure from registering its own User/Otp repositories
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    builder.Services.AddScoped<Supabase.Client>(provider =>
    {
        var options = new SupabaseOptions { AutoConnectRealtime = false };
        return new Supabase.Client(supabaseUrl, supabaseKey, options);
    });
    
    builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IUserRepository>(provider => 
    {
        var supabase = provider.GetRequiredService<Supabase.Client>();
        var logger = provider.GetRequiredService<ILogger<SupabaseUserRepository>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        return new SupabaseUserRepository(supabase, logger, configuration);
    });

    builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IOtpRepository>(provider => 
    {
        var supabase = provider.GetRequiredService<Supabase.Client>();
        var logger = provider.GetRequiredService<ILogger<SupabaseOtpRepository>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        return new SupabaseOtpRepository(logger, supabase, configuration);
    });
}

// Database configuration - check if InMemory is enabled
var useInMemory = builder.Configuration.GetConnectionString("UseInMemory") == "true";

if (useInMemory)
{
    Console.WriteLine("⚠️  WARNING: Using InMemory database (IPv6 connectivity issue with Supabase)");
    Console.WriteLine("📊 InMemory database will work for testing without external database");
    
    // Add InMemory database
    builder.Services.AddDbContext<BusTicketDbContext>(options =>
        options.UseInMemoryDatabase("BusTicketReservationDb")
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors());
    
    // Add repositories manually for InMemory
    builder.Services.AddScoped<IBusRepository, BusTicketReservation.Infrastructure.Repositories.BusRepository>();
    builder.Services.AddScoped<IRouteRepository, BusTicketReservation.Infrastructure.Repositories.RouteRepository>();
    builder.Services.AddScoped<IBusScheduleRepository, BusTicketReservation.Infrastructure.Repositories.BusScheduleRepository>();
    builder.Services.AddScoped<ISeatRepository, BusTicketReservation.Infrastructure.Repositories.SeatRepository>();
    builder.Services.AddScoped<IPassengerRepository, BusTicketReservation.Infrastructure.Repositories.PassengerRepository>();
    builder.Services.AddScoped<ITicketRepository, BusTicketReservation.Infrastructure.Repositories.TicketRepository>();
    builder.Services.AddScoped<IUnitOfWork, BusTicketReservation.Infrastructure.UnitOfWork.UnitOfWork>();
    
    // Skip auth repositories for InMemory - will be handled by existing Supabase ones
}
else
{
    // Add Infrastructure layer services (Entity Framework + PostgreSQL)
    // Skip User/Otp repository registration since we use Supabase for auth
    builder.Services.AddInfrastructure(builder.Configuration);
}

// TEMPORARILY DISABLED: Supabase Client for Authentication  
// TODO: Re-enable after testing PostgreSQL integration
/*
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    builder.Services.AddScoped<Supabase.Client>(provider =>
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };
        
        var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        return client;
    });
}

builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IUserRepository>(provider => 
{
    var supabase = provider.GetRequiredService<Supabase.Client>();
    var logger = provider.GetRequiredService<ILogger<SupabaseUserRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new SupabaseUserRepository(supabase, logger, configuration);
});

builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IOtpRepository>(provider => 
{
    var supabase = provider.GetRequiredService<Supabase.Client>();
    var logger = provider.GetRequiredService<ILogger<SupabaseOtpRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new SupabaseOtpRepository(logger, supabase, configuration);
});
*/

// TEMPORARILY DISABLED: Supabase Client for Authentication
// TODO: Re-enable after testing PostgreSQL integration
/*
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    builder.Services.AddScoped<Supabase.Client>(provider =>
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };
        
        var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        return client;
    });
}

builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IUserRepository>(provider => 
{
    var supabase = provider.GetRequiredService<Supabase.Client>();
    var logger = provider.GetRequiredService<ILogger<SupabaseUserRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new SupabaseUserRepository(supabase, logger, configuration);
});

builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IOtpRepository>(provider => 
{
    var supabase = provider.GetRequiredService<Supabase.Client>();
    var logger = provider.GetRequiredService<ILogger<SupabaseOtpRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new SupabaseOtpRepository(logger, supabase, configuration);
});
*/

// Add API-specific services
builder.Services.AddScoped<IMappingService, MappingService>();

// Add FluentValidation
builder.Services.AddScoped<IValidator<SearchBusesRequest>, SearchBusesRequestValidator>();
builder.Services.AddScoped<IValidator<BookTicketRequest>, BookTicketRequestValidator>();
builder.Services.AddScoped<IValidator<CancelTicketRequest>, CancelTicketRequestValidator>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("ApiSettings:JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "DefaultSecretKeyForDevelopment123456789");
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "BusTicketReservationAPI",
        ValidAudience = jwtSettings["Audience"] ?? "BusTicketReservationClient",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove delay of token when expire
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Bus Ticket Reservation API", 
        Version = "v1",
        Description = "API for searching and booking bus tickets with JWT Authentication"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("ApiSettings:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "http://localhost:4200", "https://localhost:4200" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed data for InMemory database
if (useInMemory)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BusTicketDbContext>();
        BusTicketReservation.Infrastructure.Data.SeedData.InitializeAsync(context).GetAwaiter().GetResult();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bus Ticket Reservation API v1");
        c.RoutePrefix = string.Empty; // Make Swagger UI the root page
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

// Add Authentication and Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Application configured successfully. Starting server...");
app.Run();
Console.WriteLine("Application stopped.");
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}
