using IsArama.Data.Context;
using IsArama.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Web.Controllers;

public class JobsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly JobDetailFetcher _fetcher;

    public JobsController(ApplicationDbContext db, JobDetailFetcher fetcher)
    {
        _db      = db;
        _fetcher = fetcher;
    }

    public async Task<IActionResult> Index(
        string? q, string? city, string? position, string? jobType, int? sourceId, int page = 1)
    {
        const int pageSize = 20;

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

        if (sourceId.HasValue)
            query = query.Where(j => j.SourceId == sourceId);

        var total = await query.CountAsync();
        var jobs = await query
            .OrderByDescending(j => j.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q          = q;
        ViewBag.City       = city;
        ViewBag.Position   = position;
        ViewBag.JobType    = jobType;
        ViewBag.SourceId   = sourceId;
        ViewBag.Page       = page;
        ViewBag.PageSize   = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total      = total;
        ViewBag.Cities       = await _db.Jobs.Select(j => j.City).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.Sources      = await _db.Sources.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        ViewBag.SourceCounts = await _db.Jobs
            .GroupBy(j => j.SourceId)
            .Select(g => new { SourceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.SourceId, g => g.Count);
        ViewBag.TotalAll = await _db.Jobs.CountAsync();

        return View(jobs);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var job = await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Source)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();

        if (string.IsNullOrWhiteSpace(job.Description))
            job.Description = await _fetcher.GetOrFetchAsync(id);

        return PartialView("_JobDetailPartial", job);
    }

    public async Task<IActionResult> GetUrl(int id)
    {
        var url = await _db.Jobs
            .Where(j => j.Id == id)
            .Select(j => j.OriginalUrl)
            .FirstOrDefaultAsync();

        return Content(url ?? string.Empty);
    }

}
