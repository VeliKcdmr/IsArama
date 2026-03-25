using IsArama.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var recentJobs = await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.Source)
            .OrderByDescending(j => j.PublishedAt)
            .Take(9)
            .ToListAsync();

        var totalJobs = await _db.Jobs.CountAsync();
        var activeSources = await _db.Sources
            .Where(s => s.IsActive && _db.Jobs.Any(j => j.SourceId == s.Id))
            .OrderByDescending(s => _db.Jobs.Count(j => j.SourceId == s.Id))
            .ToListAsync();
        var totalSources = activeSources.Count;
        var totalCompanies = await _db.Companies.CountAsync();

        ViewBag.TotalJobs     = totalJobs;
        ViewBag.TotalSources  = totalSources;
        ViewBag.TotalCompanies = totalCompanies;
        ViewBag.SourceNames   = string.Join(", ", activeSources.Take(3).Select(s => s.Name));
        ViewBag.Cities = await _db.Jobs
    .Select(j => j.City)
    .Distinct()
    .OrderBy(c => c)
    .ToListAsync();


        return View(recentJobs);
    }
}
