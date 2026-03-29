using IsArama.Data.Context;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly ScraperOrchestrator _orchestrator;
    private readonly ApplicationDbContext _db;

    public ScraperController(ScraperOrchestrator orchestrator, ApplicationDbContext db)
    {
        _orchestrator = orchestrator;
        _db = db;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run()
    {
        await _orchestrator.RunAllAsync();
        return Ok("Scraping tamamlandı.");
    }

    [HttpPost("normalize-titles")]
    public async Task<IActionResult> NormalizeTitles()
    {
        var jobs = await _db.Jobs.ToListAsync();
        int changed = 0;

        foreach (var job in jobs)
        {
            var normalized = CityNormalizer.StripLocationSuffix(job.Title);
            if (normalized != job.Title)
            {
                job.Title = normalized;
                changed++;
            }
        }

        if (changed > 0)
            await _db.SaveChangesAsync();

        return Ok($"{changed} ilanın başlığı normalize edildi.");
    }

}
