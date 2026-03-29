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
        var preferredOrder = new[] { "Kariyer.net", "Eleman.net", "Secretcv.com", "İşbul.net" };
        var displaySources = preferredOrder
            .Where(n => activeSources.Any(s => s.Name == n))
            .Take(3)
            .ToList();
        if (displaySources.Count < 3)
            displaySources.AddRange(activeSources.Select(s => s.Name).Except(displaySources).Take(3 - displaySources.Count));
        ViewBag.SourceNames = string.Join(", ", displaySources);
        ViewBag.Cities = await _db.Jobs
    .Select(j => j.City)
    .Distinct()
    .OrderBy(c => c)
    .ToListAsync();


        return View(recentJobs);
    }
}
