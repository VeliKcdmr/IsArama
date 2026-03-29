using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class LinkedInTrScraper : IScraper
{
    public string SourceName => "LinkedIn TR";
    private const string BaseUrl = "https://www.linkedin.com/jobs/search/?location=T%C3%BCrkiye&start={0}";
    private const int MaxPages = 10; // 10 x 10 = 100 ilan

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

        try
        {
            for (int page = 0; page < MaxPages; page++)
            {
                var url = string.Format(BaseUrl, page * 25);
                var doc = await web.LoadFromWebAsync(url);

                // Her job card bir <li> içindeki <a> ile birlikte
                var cards = doc.DocumentNode.SelectNodes("//ul[contains(@class,'jobs-search__results-list')]//li")
                          ?? doc.DocumentNode.SelectNodes("//ul/li[.//h3 and .//h4]");

                if (cards == null || cards.Count == 0) break;

                foreach (var card in cards)
                {
                    var link    = card.SelectSingleNode(".//a[@href]");
                    var title   = card.SelectSingleNode(".//h3");
                    var company = card.SelectSingleNode(".//h4");
                    var meta    = card.SelectSingleNode(".//span[contains(@class,'job-search-card__location')]")
                               ?? card.SelectSingleNode(".//span[@class]");
                    var logoImg = card.SelectSingleNode(".//img[contains(@class,'EntityPhoto')]")
                              ?? card.SelectSingleNode(".//img[@src and contains(@src,'media.licdn.com')]")
                              ?? card.SelectSingleNode(".//img[@data-delayed-url]");

                    if (title == null || link == null) continue;

                    var titleText   = HtmlEntity.DeEntitize(title.InnerText.Trim());
                    var companyText = company != null ? HtmlEntity.DeEntitize(company.InnerText.Trim()) : "Belirtilmemiş";
                    var href        = link.GetAttributeValue("href", "");
                    if (!href.StartsWith("http")) href = "https://www.linkedin.com" + href;
                    var logoUrl = HtmlEntity.DeEntitize(
                        logoImg?.GetAttributeValue("src", "")
                        ?? logoImg?.GetAttributeValue("data-delayed-url", "")
                        ?? "");

                    // Location genellikle "İstanbul, Türkiye" formatında
                    var locationText = meta?.InnerText.Trim() ?? "";
                    var city = CityNormalizer.Normalize(locationText);

                    // Tarih: "1 week ago", "2 days ago" gibi — card içindeki tüm text'ten bul
                    var cardText = card.InnerText;
                    var pubDate  = ParseRelativeDate(cardText);

                    jobs.Add(new JobDto
                    {
                        Title          = titleText,
                        CompanyName    = string.IsNullOrWhiteSpace(companyText) ? "Belirtilmemiş" : companyText,
                        CompanyLogoUrl = logoUrl,
                        City           = city,
                        JobType        = "Tam Zamanlı",
                        OriginalUrl    = href,
                        PublishedAt    = pubDate
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

    public Task<string?> FetchDescriptionAsync(string url)
        => Task.FromResult<string?>(null); // LinkedIn auth gerektiriyor


    private static DateTime ParseRelativeDate(string text)
    {
        var now = DateTime.UtcNow;
        var lower = text.ToLower();

        if (lower.Contains("just now") || lower.Contains("today") || lower.Contains("bugün"))
            return now;

        // "X hours ago"
        var hourMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*hour");
        if (hourMatch.Success && int.TryParse(hourMatch.Groups[1].Value, out var hours))
            return now.AddHours(-hours);

        // "X days ago"
        var dayMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*day");
        if (dayMatch.Success && int.TryParse(dayMatch.Groups[1].Value, out var days))
            return now.AddDays(-days);

        // "X weeks ago"
        var weekMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*week");
        if (weekMatch.Success && int.TryParse(weekMatch.Groups[1].Value, out var weeks))
            return now.AddDays(-weeks * 7);

        // "X months ago" or "past month"
        if (lower.Contains("month")) return now.AddMonths(-1);

        return now;
    }
}
