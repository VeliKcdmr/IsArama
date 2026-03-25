using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class MemurlarNetScraper : IScraper
{
    public string SourceName => "Memurlar.net";

    private static readonly (string Slug, string JobType)[] Categories =
    [
        ("daimi-isci-ilanlari",       "Tam Zamanlı"),
        ("sozlesmeli-personel",       "Sözleşmeli"),
        ("memur-ilanlari",            "Memur"),
        ("engelli-ilanlari",          "Tam Zamanlı"),
        ("staj-ilanlari",             "Staj"),
    ];

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        try
        {
            foreach (var (slug, jobType) in Categories)
            {
                for (int page = 1; page <= 5; page++)
                {
                    var url = page == 1
                        ? $"https://ilan.memurlar.net/kategori/{slug}/"
                        : $"https://ilan.memurlar.net/kategori/{slug}/{page}.sayfa";

                    var doc = await web.LoadFromWebAsync(url);

                    var cards = doc.DocumentNode.SelectNodes(
                        "//div[contains(@class,'content-items') and contains(@class,'list')]//a[contains(@href,'/ilan/')]");

                    if (cards == null || !cards.Any()) break;

                    foreach (var card in cards)
                    {
                        var title = System.Net.WebUtility.HtmlDecode(
                            card.GetAttributeValue("title", "")?.Trim() ?? "");
                        var link = card.GetAttributeValue("href", "");

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                        // Date: first text node inside the <a> (e.g. "23 Mart")
                        var dateText = card.SelectSingleNode(".//text()[normalize-space()!='']")
                                          ?.InnerText?.Trim() ?? "";

                        // City: extract first word of title (typically the province name)
                        var city = CityNormalizer.Normalize(ExtractCity(title));

                        jobs.Add(new JobDto
                        {
                            Title          = title,
                            CompanyName    = title,   // title contains org name; no separate field
                            CompanyLogoUrl = "",
                            City           = city,
                            JobType        = jobType,
                            OriginalUrl    = link.StartsWith("http") ? link
                                           : link.StartsWith("//")   ? "https:" + link
                                           : $"https://ilan.memurlar.net{link}",
                            PublishedAt    = ParseDate(dateText)
                        });
                    }

                    await Task.Delay(1000);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{SourceName}] Hata: {ex.Message}");
        }

        return jobs;
    }

    private static string ExtractCity(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Belirtilmemiş";
        // Title format is usually "CityName District/OrgName X workers ..."
        // First word is typically the province
        var firstSpace = title.IndexOf(' ');
        var city = firstSpace > 0 ? title[..firstSpace].Trim() : title.Trim();
        return string.IsNullOrWhiteSpace(city) ? "Belirtilmemiş" : city;
    }

    private static DateTime ParseDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DateTime.UtcNow;
        text = text.Trim();

        var months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ocak"]    = 1,  ["Şubat"]  = 2,  ["Mart"]    = 3,
            ["Nisan"]   = 4,  ["Mayıs"]  = 5,  ["Haziran"] = 6,
            ["Temmuz"]  = 7,  ["Ağustos"] = 8, ["Eylül"]   = 9,
            ["Ekim"]    = 10, ["Kasım"]  = 11, ["Aralık"]  = 12,
        };

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2
            && int.TryParse(parts[0], out int day)
            && months.TryGetValue(parts[1], out int month))
        {
            var year = DateTime.UtcNow.Year;
            // If month is in the future, it belongs to last year
            if (month > DateTime.UtcNow.Month) year--;
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        return DateTime.UtcNow;
    }
}
