using IsArama.Api.Services;
using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly JobDetailFetcher _fetcher;

    public JobsController(ApplicationDbContext db, JobDetailFetcher fetcher)
    {
        _db = db;
        _fetcher = fetcher;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? q,
        [FromQuery] string? city,
        [FromQuery] string? position,
        [FromQuery] string? jobType,
        [FromQuery] string? source,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Source)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(j => j.Title.Contains(q) || j.Company.Name.Contains(q));

        if (!string.IsNullOrWhiteSpace(position))
            query = query.Where(j => j.Title.Contains(position));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(j => j.City == city);

        if (!string.IsNullOrWhiteSpace(jobType))
            query = query.Where(j => j.JobType == jobType);

        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(j => j.Source.Name == source);

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
                Source = j.Source.Name,
                SourceUrl = j.Source.BaseUrl
            })
            .FirstOrDefaultAsync();

        if (job == null) return NotFound();

        // Description yoksa orijinal siteden çek ve DB'ye kaydet
        if (string.IsNullOrWhiteSpace(job.Description))
            await _fetcher.GetOrFetchAsync(id);

        // Güncel description DB'den oku (fetcher kaydettiyse artık dolu)
        var freshDesc = await _db.Jobs
            .Where(j => j.Id == id)
            .Select(j => j.Description)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            job.Id,
            job.Title,
            Description = freshDesc,
            job.City,
            job.JobType,
            job.OriginalUrl,
            job.PublishedAt,
            Company     = job.Company,
            CompanyLogo = job.CompanyLogo,
            Source      = job.Source,
            SourceUrl   = job.SourceUrl
        });
    }
}
