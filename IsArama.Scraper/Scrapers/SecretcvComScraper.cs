using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class SecretcvComScraper : IScraper
{
    public string SourceName => "Secretcv.com";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        try
        {
            for (int page = 1; page <= 25; page++)
            {
                var url = page == 1
                    ? "https://www.secretcv.com/is-ilanlari"
                    : $"https://www.secretcv.com/is-ilanlari/{page}";

                var doc = await web.LoadFromWebAsync(url);

                var cards = doc.DocumentNode.SelectNodes(
                    "//div[contains(@class,'cv-job-box') and contains(@class,'job-list')]");

                if (cards == null || !cards.Any()) break;

                foreach (var card in cards)
                {
                    // Başlık ve URL
                    var linkNode = card.SelectSingleNode(".//a[contains(@class,'title')]");
                    var title = linkNode?.GetAttributeValue("title", "")?.Trim();
                    var link  = linkNode?.GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                    title = System.Net.WebUtility.HtmlDecode(title);

                    // Şirket
                    var company = System.Net.WebUtility.HtmlDecode(
                        card.SelectSingleNode(".//a[contains(@class,'company')]")
                            ?.GetAttributeValue("title", "") ?? "Belirtilmemiş");

                    // Şehir — city span içindeki ilk span'dan al
                    var cityNode = card.SelectSingleNode(".//span[contains(@class,'city')]//span[not(contains(@class,'text-muted'))]");
                    var city = System.Net.WebUtility.HtmlDecode(cityNode?.InnerText ?? "")
                        .Replace("\n", "").Replace("\r", "").Trim();
                    // İkon karakterlerini temizle
                    city = CityNormalizer.Normalize(System.Text.RegularExpressions.Regex.Replace(city, @"\s+", " ").Trim());

                    // Logo — lazy-load için data-src
                    var logoUrl = card.SelectSingleNode(".//img[contains(@class,'img-brand')]")
                                      ?.GetAttributeValue("data-src", "") ?? "";

                    // Tarih
                    var dateText = card.SelectSingleNode(".//small[contains(@class,'text-muted')]")
                                       ?.InnerText.Trim() ?? "";

                    jobs.Add(new JobDto
                    {
                        Title          = title,
                        CompanyName    = string.IsNullOrWhiteSpace(company) ? "Belirtilmemiş" : company,
                        CompanyLogoUrl = logoUrl,
                        City           = string.IsNullOrWhiteSpace(city) ? "Belirtilmemiş" : city,
                        JobType        = "Tam Zamanlı",
                        OriginalUrl    = link.StartsWith("http") ? link : $"https://www.secretcv.com{link}",
                        PublishedAt    = ParseDate(dateText)
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

    public async Task<string?> FetchDescriptionAsync(string url)
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(15);
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        try
        {
            var html = await http.GetStringAsync(url);
            var doc  = new HtmlDocument();
            doc.LoadHtml(html);
            return DescriptionFetcher.TryXPaths(doc,
                "//div[contains(@class,'job-detail-description')]",
                "//div[contains(@class,'detail-description')]",
                "//div[contains(@class,'cv-job-description')]",
                "//div[contains(@class,'job-description')]",
                "//div[contains(@class,'description-content')]",
                "//section[contains(@class,'description')]",
                "//div[@id='jobDescription']",
                "//div[contains(@class,'detail-content')]");
        }
        catch { return null; }
    }

    private static DateTime ParseDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DateTime.UtcNow;
        text = text.ToLower();
        if (text.Contains("bugün") || text.Contains("today")) return DateTime.UtcNow;
        if (text.Contains("dün") || text.Contains("yesterday")) return DateTime.UtcNow.AddDays(-1);

        // "2 month 11 day ago" gibi
        var monthMatch = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)\s*month");
        var dayMatch   = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)\s*day");
        int days = 0;
        if (monthMatch.Success) days += int.Parse(monthMatch.Groups[1].Value) * 30;
        if (dayMatch.Success)   days += int.Parse(dayMatch.Groups[1].Value);
        if (days > 0) return DateTime.UtcNow.AddDays(-days);

        return DateTime.UtcNow;
    }
}
