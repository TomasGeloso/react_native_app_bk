using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using react_native_app_bk.Data;
using react_native_app_bk.Logging;
using react_native_app_bk.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT key is missing. The application cannot function without a valid key.");
}

// Register custom console formatter for logging
builder.Services.AddSingleton<ConsoleFormatter, CustomConsoleFormatter>();

// Configure Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole(options =>
    {
        options.FormatterName = "CustomFormatter";
    });
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
    logging.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ActionResultExecutor", LogLevel.Information);
    logging.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogLevel.Information);
});

// DbContext and MySQL Configuration
try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<AppDbContext>(dbContextOptions => dbContextOptions
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        // The following lines are only for debugging. Change for production.
        //.EnableSensitiveDataLogging()
        //.EnableDetailedErrors()
        );
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();
    logger.LogError(ex, "Database connection failed during application startup.");
    throw new InvalidOperationException("Failed to connect to the database. See inner exception for details.", ex);
}


// User Service Registration
builder.Services.AddScoped<IUserService, UserService>();

// Sample Service Registration
builder.Services.AddScoped<ISampleService, SampleService>();

// Refresh Token Service Registration
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Material Service Registration
builder.Services.AddScoped<IMaterialService, MaterialService>();

// Sample Type Service Registration
builder.Services.AddScoped<ISampleTypeService, SampleTypeService>();

// Test Specimen Type Service Registration
builder.Services.AddScoped<ITestSpecimenTypeService, TestSpecimenTypeService>();


// Authorization Configuration
builder.Services.AddAuthorization();

// JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Program>();
                logger.LogError("Token has Expired!");
                context.Response.Headers["Token-Expired"] = "true";
            }
            return Task.CompletedTask;
        }
    };
});


// Cors Configuration

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:8081", "http://192.168.1.50:8081")    // React Native App
            .AllowAnyMethod()  // Allow any http method
            .AllowAnyHeader()   // Allow any header
            .AllowCredentials() // Allow credentials
            .WithExposedHeaders("Token-Expired"); // Allow Authorization header
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "react_native_app_bk", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' following by space and JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "react_native_app_bk v1");
    });
}

//app.UseDeveloperExceptionPage();

//app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


await app.RunAsync();
