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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Supabase Client
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    builder.Services.AddScoped<Supabase.Client>(provider =>
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };
        
        var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        client.InitializeAsync().Wait(); // Note: In production, consider async initialization
        return client;
    });
}

// Add Application layer services
builder.Services.AddApplication();

// Add basic services for testing - TEMPORARILY DISABLED
// builder.Services.AddScoped<BusTicketReservation.WebApi.Services.SupabaseService>();

// Add Infrastructure layer services - TEMPORARILY DISABLED due to EF version conflicts
// builder.Services.AddInfrastructure(builder.Configuration);

// Add Supabase-based repositories for authentication
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

// Add placeholder for IUnitOfWork (not used in Supabase implementation)
builder.Services.AddScoped<BusTicketReservation.Application.Interfaces.IUnitOfWork, NoOpUnitOfWork>();

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

app.Run();
