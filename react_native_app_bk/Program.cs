using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using react_native_app_bk.Data;
using react_native_app_bk.Services;
using System.Text;
using react_native_app_bk.Logging;

var builder = WebApplication.CreateBuilder(args);

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
    logging.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogLevel.Information); // Suppress controller action invoker logs
});

// DbContext and MySQL Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>( dbContextOptions => dbContextOptions
    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
    // The following lines are only for debugging. Change for production.
    //.EnableSensitiveDataLogging()
    //.EnableDetailedErrors()
    );

// User Service Registration
builder.Services.AddScoped<IUserService, UserService>();

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
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Cors Configuration

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:8081")    // React Native App
            .AllowAnyMethod()  // Allow any http method
            .AllowAnyHeader();   // Allow any header
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
