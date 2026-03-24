using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Web.Controllers;

public class JobsController : Controller
{
    private readonly ApplicationDbContext _db;

    public JobsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        string? q, string? city, int? categoryId, string? jobType, int page = 1)
    {
        const int pageSize = 20;

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
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.City = city;
        ViewBag.CategoryId = categoryId;
        ViewBag.JobType = jobType;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total = total;
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Cities = await _db.Jobs.Select(j => j.City).Distinct().OrderBy(c => c).ToListAsync();

        return View(jobs);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var job = await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.Source)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();
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
