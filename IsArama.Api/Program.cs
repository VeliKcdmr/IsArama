using Hangfire;
using Hangfire.SqlServer;
using IsArama.Api.Services;
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
builder.Services.AddScoped<IScraper, MemurlarNetScraper>();
builder.Services.AddScoped<IScraper, LinkedInTrScraper>();
builder.Services.AddScoped<IScraper, IsbulNetScraper>();
builder.Services.AddScoped<IScraper, ElemanNetScraper>();
builder.Services.AddScoped<IScraper, YenibirisComScraper>();
builder.Services.AddScoped<IScraper, SecretcvComScraper>();
builder.Services.AddScoped<IScraper, KariyerNetScraper>();
builder.Services.AddScoped<HashService>();
builder.Services.AddScoped<ScraperOrchestrator>();
builder.Services.AddScoped<JobDetailFetcher>();

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
        policy.WithOrigins(
                "https://isarama.com.tr",
                "https://www.isarama.com.tr",
                "https://api.isarama.com.tr")
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

// Hangfire Dashboard — sadece local erişime açık
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

// Her 30 dakikada bir scrape
RecurringJob.AddOrUpdate<ScraperOrchestrator>(
    "scrape-all",
    x => x.RunAllAsync(),
    "*/30 * * * *");

app.UseCors("AllowAll");
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
