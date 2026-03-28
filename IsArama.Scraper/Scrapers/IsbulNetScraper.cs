using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class IsbulNetScraper : IScraper
{
    public string SourceName => "İşbul.net";
    private const string BaseUrl = "https://www.isbul.net/is-ilanlari?page={0}";
    private const int MaxPages = 25;

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

        try
        {
            for (int page = 1; page <= MaxPages; page++)
            {
                var url = string.Format(BaseUrl, page);
                var doc = await web.LoadFromWebAsync(url);

                // Tüm ilan linklerini seç, href'e göre deduplicate et
                var allCards = doc.DocumentNode.SelectNodes("//a[contains(@href,'/is-ilani/')]");
                if (allCards == null || allCards.Count == 0) break;

                // Her href için sadece bir kart al (desktop+mobile çift geliyor)
                var seen = new HashSet<string>();
                var cards = allCards
                    .Where(c => seen.Add(c.GetAttributeValue("href", "")))
                    .ToList();

                foreach (var card in cards)
                {
                    var href = card.GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(href)) continue;
                    var fullUrl = "https://www.isbul.net" + href;

                    // Başlık: title attribute'u olan ilk div
                    var titleNode = card.SelectSingleNode(".//div[@title]");
                    var title = titleNode?.GetAttributeValue("title", "")
                             ?? titleNode?.InnerText.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    // Şirket: title attribute'u olan p tag'ı
                    var companyNode = card.SelectSingleNode(".//p[@title]");
                    var company = companyNode?.GetAttributeValue("title", "")
                               ?? companyNode?.InnerText.Trim() ?? "Belirtilmemiş";
                    if (company.ToLower().Contains("gizli")) company = "Gizli Firma";

                    // Logo: img src (avatar değilse al)
                    var imgNode = card.SelectSingleNode(".//img[@src]");
                    var logoUrl = imgNode?.GetAttributeValue("src", "") ?? "";

                    // Span'lar: "|" ayraçları hariç tut
                    // Sıra: [0]=şehir, [1]=iş tipi, [2]=çalışma şekli, [last]=tarih
                    var spans = card.SelectNodes(".//span")
                        ?.Select(s => HtmlEntity.DeEntitize(s.InnerText.Trim()))
                        .Where(t => t != "|" && !string.IsNullOrWhiteSpace(t))
                        .ToList() ?? [];

                    var city    = spans.Count > 0 ? CityNormalizer.Normalize(spans[0]) : "Belirtilmemiş";
                    var jobType = spans.Count > 1 ? NormalizeJobType(spans[1]) : "Tam Zamanlı";
                    var dateStr = spans.Count > 0 ? spans[^1] : "";
                    var pubDate = ParseRelativeDate(dateStr);

                    jobs.Add(new JobDto
                    {
                        Title          = HtmlEntity.DeEntitize(title),
                        CompanyName    = string.IsNullOrWhiteSpace(company) ? "Belirtilmemiş" : company,
                        CompanyLogoUrl = logoUrl,
                        City           = city,
                        JobType        = jobType,
                        OriginalUrl    = fullUrl,
                        PublishedAt    = pubDate
                    });
                }

                await Task.Delay(1000);
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
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        try
        {
            var html = await http.GetStringAsync(url);
            var doc  = new HtmlDocument();
            doc.LoadHtml(html);
            return DescriptionFetcher.TryXPaths(doc,
                "//div[contains(@class,'job-detail__description')]",
                "//div[contains(@class,'detail-job__desc')]",
                "//div[contains(@class,'job-description')]",
                "//div[contains(@class,'detail-content')]",
                "//section[contains(@class,'job-detail')]",
                "//div[@id='jobDesc']",
                "//div[@id='job-detail-content']",
                "//div[contains(@class,'ilan-detay')]");
        }
        catch { return null; }
    }

    private static string NormalizeJobType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Tam Zamanlı";
        var lower = raw.ToLower();
        if (lower.Contains("part") || lower.Contains("yarı")) return "Yarı Zamanlı";
        if (lower.Contains("staj") || lower.Contains("intern")) return "Staj";
        if (lower.Contains("uzaktan") || lower.Contains("remote")) return "Uzaktan";
        if (lower.Contains("freelance") || lower.Contains("serbest")) return "Freelance";
        return "Tam Zamanlı";
    }

    private static DateTime ParseRelativeDate(string text)
    {
        var now = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(text)) return now;
        var lower = text.ToLower();

        var m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*gün");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var d)) return now.AddDays(-d);

        m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*saat");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var h)) return now.AddHours(-h);

        m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*hafta");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var w)) return now.AddDays(-w * 7);

        m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*ay");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var mo)) return now.AddMonths(-mo);

        return now;
    }
}
