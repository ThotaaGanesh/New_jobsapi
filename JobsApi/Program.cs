using JobsApi.Extensions;
using JobsApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<JobsApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();  // Add this line

//Registering services to DI
builder.Services.RegisterService();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = builder.Configuration["AppSettings:JwtConfig:ValidIssuer"],
                       ValidAudience = builder.Configuration["AppSettings:JwtConfig:ValidAudience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[key: "AppSettings:JwtConfig:Secret"]))
                   };
               });
Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console()
                .WriteTo.File("logs/JobsApi.txt")
                .CreateLogger();
// Set Serilog as the logging provider
// This will also replace default logging provider with Serilog
builder.Host.UseSerilog();// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();  // Add this line
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(builder =>
{
    builder.AllowAnyOrigin() // Allowed origins
           .AllowAnyHeader() // Allow any headers
           .AllowAnyMethod(); // Allow any methods (GET, POST, etc.)
});

app.MapControllers();

app.Run();
