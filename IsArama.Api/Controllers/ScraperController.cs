using IsArama.Scraper.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly ScraperOrchestrator _orchestrator;

    public ScraperController(ScraperOrchestrator orchestrator)
        => _orchestrator = orchestrator;

    [HttpPost("run")]
    public async Task<IActionResult> Run()
    {
        await _orchestrator.RunAllAsync();
        return Ok("Scraping tamamlandı.");
    }
}
