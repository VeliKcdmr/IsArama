using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public SourcesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetSources()
    {
        var sources = await _db.Sources
            .Where(s => s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.BaseUrl,
                JobCount = s.Jobs.Count()
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(sources);
    }
}
