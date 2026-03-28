using IsArama.Data.Context;
using IsArama.Data.Entities;
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
        // Tüm mevcut hash'leri başta bir kez çek
        var existingHashes = (await _db.Jobs
            .Select(j => j.Hash).ToListAsync()).ToHashSet();

        // Company cache
        var companyCache = await _db.Companies
            .ToDictionaryAsync(c => c.Name, c => c);

        foreach (var scraper in _scrapers)
        {
            var source = await _db.Sources
                .FirstOrDefaultAsync(s => s.Name == scraper.SourceName && s.IsActive);
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

                // DB'ye gitmeden in-memory kontrol
                if (existingHashes.Contains(hash)) continue;

                if (string.IsNullOrWhiteSpace(dto.Description))
                {
                    try { dto.Description = await scraper.FetchDescriptionAsync(dto.OriginalUrl); }
                    catch { }
                    await Task.Delay(100);
                }

                // Company cache'den bak
                if (!companyCache.TryGetValue(dto.CompanyName ?? "", out var company))
                {
                    company = new Company { Name = dto.CompanyName ?? "", LogoUrl = dto.CompanyLogoUrl };
                    _db.Companies.Add(company);
                    await _db.SaveChangesAsync();
                    companyCache[company.Name] = company;
                }
                else if (string.IsNullOrWhiteSpace(company.LogoUrl) && !string.IsNullOrWhiteSpace(dto.CompanyLogoUrl))
                {
                    company.LogoUrl = dto.CompanyLogoUrl;
                    await _db.SaveChangesAsync();
                }

                if (existingHashes.Contains(hash)) continue;

                _db.Jobs.Add(new Job
                {
                    Title       = (dto.Title?.Length > 300 ? dto.Title[..300] : dto.Title) ?? "",
                    Description = dto.Description,
                    City        = (dto.City?.Length > 100 ? dto.City[..100] : dto.City) ?? "Belirtilmemiş",
                    JobType     = (dto.JobType?.Length > 50 ? dto.JobType[..50] : dto.JobType) ?? "Tam Zamanlı",
                    OriginalUrl = dto.OriginalUrl,
                    Hash        = hash,
                    PublishedAt = dto.PublishedAt,
                    CompanyId   = company.Id,
                    SourceId    = source.Id
                });

                try
                {
                    await _db.SaveChangesAsync();
                    existingHashes.Add(hash); // cache'e ekle
                    savedCount++;
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                    when (ex.InnerException?.Message.Contains("duplicate key") == true ||
                          ex.InnerException?.Message.Contains("IX_Jobs_Hash") == true)
                {
                    existingHashes.Add(hash);
                    _db.ChangeTracker.Clear();
                }
            }

            Console.WriteLine($"[{scraper.SourceName}] {savedCount} yeni ilan kaydedildi.");
        }
    }

}
