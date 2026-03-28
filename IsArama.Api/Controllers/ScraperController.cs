using IsArama.Data.Context;
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
