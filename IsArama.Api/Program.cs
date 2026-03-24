using Hangfire;
using Hangfire.SqlServer;
using IsArama.Data.Context;
using IsArama.Scraper.Interfaces;
using IsArama.Scraper.Scrapers;
using IsArama.Scraper.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Scrapers
builder.Services.AddScoped<IScraper, KariyerNetScraper>();
builder.Services.AddScoped<IScraper, ElemanNetScraper>();
builder.Services.AddScoped<HashService>();
builder.Services.AddScoped<ScraperOrchestrator>();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Her 3 saatte bir scrape
RecurringJob.AddOrUpdate<ScraperOrchestrator>(
    "scrape-all",
    x => x.RunAllAsync(),
    "0 */3 * * *");

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
