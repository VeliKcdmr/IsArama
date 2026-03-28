using IsArama.Scraper.Dto;

namespace IsArama.Scraper.Interfaces;

public interface IScraper
{
    string SourceName { get; }
    Task<List<JobDto>> ScrapeAsync();

    /// <summary>
    /// Verilen URL için ilan açıklamasını çeker. Yeni ilanlar için orchestrator çağırır.
    /// </summary>
    Task<string?> FetchDescriptionAsync(string url);
}
