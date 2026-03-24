using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CitiesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _db.Jobs
            .Select(j => j.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(cities);
    }
}
