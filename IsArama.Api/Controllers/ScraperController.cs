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

    [HttpPost("reclassify")]
    public async Task<IActionResult> Reclassify()
    {
        var categories = await _db.Categories.ToListAsync();
        var other = categories.First(c => c.Name == "Diğer");
        var categoryMap = categories.ToDictionary(c => c.Name, c => c.Id);

        var jobs = await _db.Jobs.ToListAsync();
        int updated = 0;

        foreach (var job in jobs)
        {
            var categoryName = CategoryClassifier.Classify(job.Title ?? "");
            if (!categoryMap.TryGetValue(categoryName, out var catId))
                catId = other.Id;

            if (job.CategoryId != catId)
            {
                job.CategoryId = catId;
                updated++;
            }
        }

        await _db.SaveChangesAsync();
        return Ok($"{updated} ilan yeniden sınıflandırıldı.");
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var jobCount = await _db.Jobs.CountAsync();
        var companyCount = await _db.Companies.CountAsync();
        await _db.Jobs.ExecuteDeleteAsync();
        await _db.Companies.ExecuteDeleteAsync();
        return Ok($"{jobCount} ilan ve {companyCount} şirket silindi.");
    }
}
