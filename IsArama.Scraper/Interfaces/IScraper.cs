using IsArama.Scraper.Dto;

namespace IsArama.Scraper.Interfaces;

public interface IScraper
{
    string SourceName { get; }
    Task<List<JobDto>> ScrapeAsync();
}
