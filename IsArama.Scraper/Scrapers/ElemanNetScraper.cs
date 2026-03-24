using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class ElemanNetScraper : IScraper
{
    public string SourceName => "Eleman.net";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        try
        {
            for (int page = 1; page <= 5; page++)
            {
                var url = $"https://www.eleman.net/is-ilanlari?page={page}";
                var doc = await web.LoadFromWebAsync(url);

                var nodes = doc.DocumentNode
                    .SelectNodes("//div[contains(@class,'job-list')]//div[contains(@class,'job-item')]");

                if (nodes == null) break;

                foreach (var node in nodes)
                {
                    var title = node.SelectSingleNode(".//h2|.//h3")?.InnerText.Trim();
                    var company = node.SelectSingleNode(".//*[contains(@class,'company')]")?.InnerText.Trim();
                    var city = node.SelectSingleNode(".//*[contains(@class,'location')]")?.InnerText.Trim();
                    var link = node.SelectSingleNode(".//a")?.GetAttributeValue("href", "");

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                    jobs.Add(new JobDto
                    {
                        Title = title,
                        CompanyName = company ?? "Belirtilmemiş",
                        City = city ?? "Belirtilmemiş",
                        OriginalUrl = link.StartsWith("http") ? link : $"https://www.eleman.net{link}",
                        PublishedAt = DateTime.UtcNow
                    });
                }

                await Task.Delay(1500);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{SourceName}] Hata: {ex.Message}");
        }

        return jobs;
    }
}
