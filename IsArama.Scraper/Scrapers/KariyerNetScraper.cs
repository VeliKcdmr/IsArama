using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class KariyerNetScraper : IScraper
{
    public string SourceName => "Kariyer.net";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";

        try
        {
            for (int page = 1; page <= 5; page++)
            {
                var url = $"https://www.kariyer.net/is-ilanlari?page={page}";
                var doc = await web.LoadFromWebAsync(url);

                var nodes = doc.DocumentNode
                    .SelectNodes("//div[@data-test='ad-card']");

                if (nodes == null || !nodes.Any()) break;

                foreach (var node in nodes)
                {
                    // Data attribute'lardan çek
                    var title = node.GetAttributeValue("positionname", "").Trim();
                    var city = node.GetAttributeValue("cityname", "").Trim();
                    var workType = node.GetAttributeValue("worktypetext", "Tam Zamanlı").Trim();

                    // Şirket adı
                    var company = node.SelectSingleNode(".//span[@data-test='subtitle']")
                                  ?.InnerText.Trim();

                    // Link
                    var link = node.SelectSingleNode(".//a[@data-test='ad-card-item']")
                               ?.GetAttributeValue("href", "");

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                    jobs.Add(new JobDto
                    {
                        Title = title,
                        CompanyName = string.IsNullOrWhiteSpace(company) ? "Belirtilmemiş" : company,
                        City = string.IsNullOrWhiteSpace(city) ? "Belirtilmemiş" : city,
                        JobType = workType,
                        OriginalUrl = link.StartsWith("http") ? link : $"https://www.kariyer.net{link}",
                        PublishedAt = ParseKariyerDate(node.GetAttributeValue("time", ""))
                    });

                }

                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{SourceName}] Hata: {ex.Message}");
        }

        return jobs;
    }

    private static DateTime ParseKariyerDate(string timeText)
    {
        if (string.IsNullOrWhiteSpace(timeText)) return DateTime.UtcNow;

        timeText = timeText.Trim().ToLower();

        if (timeText.Contains("bugün") || timeText.Contains("saat") || timeText.Contains("dakika"))
            return DateTime.UtcNow;

        if (timeText.Contains("dün"))
            return DateTime.UtcNow.AddDays(-1);

        // "3 gün", "5 gün" gibi
        var parts = timeText.Split(' ');
        if (parts.Length >= 2 && int.TryParse(parts[0], out int days) && parts[1].Contains("gün"))
            return DateTime.UtcNow.AddDays(-days);

        // "2 hafta"
        if (parts.Length >= 2 && int.TryParse(parts[0], out int weeks) && parts[1].Contains("hafta"))
            return DateTime.UtcNow.AddDays(-weeks * 7);

        return DateTime.UtcNow;
    }

}
