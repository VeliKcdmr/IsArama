using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PositionsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetPositions()
    {
        var raw = await _db.Jobs
            .GroupBy(j => j.Title)
            .OrderByDescending(g => g.Count())
            .Take(300)
            .Select(g => g.Key)
            .ToListAsync();

        var positions = raw
            .Select(t => IsArama.Scraper.Helpers.CityNormalizer.StripLocationSuffix(t))
            .Where(t => !string.IsNullOrWhiteSpace(t) && !t.Any(c => c >= 0x0600 && c <= 0x06FF))
            .Distinct()
            .OrderBy(t => t)
            .Take(150)
            .ToList();

        return Ok(positions);
    }
}
