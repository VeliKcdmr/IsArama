using IsArama.Data.Context;
using IsArama.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public JobsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? q,
        [FromQuery] string? city,
        [FromQuery] int? categoryId,
        [FromQuery] string? jobType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.Source)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(j => j.Title.Contains(q) || j.Company.Name.Contains(q));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(j => j.City == city);

        if (categoryId.HasValue)
            query = query.Where(j => j.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(jobType))
            query = query.Where(j => j.JobType == jobType);

        var total = await query.CountAsync();

        var jobs = await query
            .OrderByDescending(j => j.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new
            {
                j.Id,
                j.Title,
                j.City,
                j.JobType,
                j.PublishedAt,
                Company = j.Company.Name,
                CompanyLogo = j.Company.LogoUrl,
                Category = j.Category.Name,
                Source = j.Source.Name
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, jobs });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.Source)
            .Where(j => j.Id == id)
            .Select(j => new
            {
                j.Id,
                j.Title,
                j.Description,
                j.City,
                j.JobType,
                j.OriginalUrl,
                j.PublishedAt,
                Company = j.Company.Name,
                CompanyLogo = j.Company.LogoUrl,
                Category = j.Category.Name,
                Source = j.Source.Name,
                SourceUrl = j.Source.BaseUrl
            })
            .FirstOrDefaultAsync();

        if (job == null) return NotFound();
        return Ok(job);
    }
}
