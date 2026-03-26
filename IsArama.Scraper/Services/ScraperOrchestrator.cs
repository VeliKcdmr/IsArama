using IsArama.Data.Context;
using IsArama.Data.Entities;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Scraper.Services;

public class ScraperOrchestrator
{
    private readonly ApplicationDbContext _db;
    private readonly IEnumerable<IScraper> _scrapers;
    private readonly HashService _hashService;

    public ScraperOrchestrator(ApplicationDbContext db, IEnumerable<IScraper> scrapers, HashService hashService)
    {
        _db = db;
        _scrapers = scrapers;
        _hashService = hashService;
    }

    public async Task RunAllAsync()
    {
        foreach (var scraper in _scrapers)
        {
            var source = await _db.Sources.FirstOrDefaultAsync(s => s.Name == scraper.SourceName && s.IsActive);
            if (source == null) continue;

            Console.WriteLine($"[{scraper.SourceName}] Scraping başladı...");

            var jobs = await scraper.ScrapeAsync();

            var processedHashes = new HashSet<string>();
            int savedCount = 0;

            foreach (var dto in jobs)
            {
                var hash = _hashService.Compute(dto.Title ?? "", dto.CompanyName ?? "", dto.City ?? "");

                if (processedHashes.Contains(hash)) continue;
                processedHashes.Add(hash);

                if (await _db.Jobs.AnyAsync(j => j.Hash == hash)) continue;

                var company = await _db.Companies.FirstOrDefaultAsync(c => c.Name == dto.CompanyName);
                if (company == null)
                {
                    company = new Company { Name = dto.CompanyName ?? "", LogoUrl = dto.CompanyLogoUrl };
                    _db.Companies.Add(company);
                    await _db.SaveChangesAsync();
                }
                else if (string.IsNullOrWhiteSpace(company.LogoUrl) && !string.IsNullOrWhiteSpace(dto.CompanyLogoUrl))
                {
                    company.LogoUrl = dto.CompanyLogoUrl;
                    await _db.SaveChangesAsync();
                }

                var categoryName = CategoryClassifier.Classify(dto.Title ?? "");
                var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == categoryName)
                               ?? await _db.Categories.FirstAsync(c => c.Name == "Diğer");

                _db.Jobs.Add(new Job
                {
                    Title = (dto.Title?.Length > 300 ? dto.Title[..300] : dto.Title) ?? "",
                    Description = dto.Description,
                    City = (dto.City?.Length > 100 ? dto.City[..100] : dto.City) ?? "Belirtilmemiş",
                    JobType = (dto.JobType?.Length > 50 ? dto.JobType[..50] : dto.JobType) ?? "Tam Zamanlı",
                    OriginalUrl = dto.OriginalUrl,
                    Hash = hash,
                    PublishedAt = dto.PublishedAt,
                    CompanyId = company.Id,
                    CategoryId = category.Id,
                    SourceId = source.Id
                });

                await _db.SaveChangesAsync();
                savedCount++;
            }

            Console.WriteLine($"[{scraper.SourceName}] {savedCount} yeni ilan kaydedildi.");
        }
    }
}
