using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class ElemanNetScraper : IScraper
{
    public string SourceName => "Eleman.net";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";

        for (int page = 1; page <= 5; page++)
        {
            try
            {
                var url = $"https://www.eleman.net/is-ilanlari?page={page}";
                var doc = await web.LoadFromWebAsync(url);

                // href="/is-ilani/" içeren ve title attribute'u olan tüm linkler
                var nodes = doc.DocumentNode
                    .SelectNodes("//a[contains(@href,'/is-ilani/') and @title and @target='_blank']");

                if (nodes == null || !nodes.Any()) break;

                foreach (var node in nodes)
                {
                    var link = node.GetAttributeValue("href", "");

                    // Başlık — h3 içinden
                    var title = node.SelectSingleNode(".//h3[contains(@class,'c-showcase-box__title')]")
                                ?.InnerText.Trim();

                    // Şirket + Şehir — subtitle içinden
                    var subtitleNode = node.SelectSingleNode(".//*[contains(@class,'c-showcase-box__subtitle')]");
                    string company = "Belirtilmemiş";
                    string city = "Belirtilmemiş";

                    if (subtitleNode != null)
                    {
                        var text = subtitleNode.InnerText.Trim();
                        var parts = text.Split('-');
                        if (parts.Length >= 1) company = parts[0].Trim().Trim('"');
                        if (parts.Length >= 2) city = CityNormalizer.Normalize(parts[1]);
                    }

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                    // Logo — src yoksa data-src'ye bak
                    var logoNode = node.SelectSingleNode(".//img[contains(@class,'c-brand-logo__img')]");
                    var logoUrl = logoNode?.GetAttributeValue("src", "");
                    if (string.IsNullOrWhiteSpace(logoUrl))
                        logoUrl = logoNode?.GetAttributeValue("data-src", "");

                    // İş tipi — u-align-middle span'larından iş tipi içeren birini bul
                    string jobType = "Tam Zamanlı";
                    var alignSpans = node.SelectNodes(".//span[contains(@class,'u-align-middle')]");
                    if (alignSpans != null)
                    {
                        foreach (var span in alignSpans)
                        {
                            var t = System.Net.WebUtility.HtmlDecode(span.InnerText.Trim());
                            if (t.Contains("Tam") || t.Contains("Yarı") || t.Contains("Part Time") ||
                                t.Contains("Staj") || t.Contains("Uzaktan") || t.Contains("Freelance") ||
                                t.Contains("Dönemsel") || t.Contains("Serbest"))
                            {
                                jobType = NormalizeJobType(t);
                                break;
                            }
                        }
                    }

                    // Tarih — listeleme sayfasında genellikle yoktur, UtcNow kullan
                    var dateSpan = node.SelectSingleNode(".//*[contains(@class,'date') or contains(@class,'time')]");
                    var pubDate  = ParseDate(dateSpan?.InnerText.Trim() ?? "");

                    jobs.Add(new JobDto
                    {
                        Title          = CityNormalizer.StripLocationSuffix(title),
                        CompanyName    = company,
                        CompanyLogoUrl = logoUrl ?? "",
                        City           = city,
                        JobType        = jobType,
                        OriginalUrl    = link.StartsWith("http") ? link : $"https://www.eleman.net{link}",
                        PublishedAt    = pubDate
                    });
                }

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{SourceName}] Sayfa {page} atlandı: {ex.Message}");
                break;
            }
        }

        return jobs;
    }

    public async Task<string?> FetchDescriptionAsync(string url)
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(15);
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");
        try
        {
            var html = await http.GetStringAsync(url);
            var doc  = new HtmlDocument();
            doc.LoadHtml(html);
            return DescriptionFetcher.TryXPaths(doc,
                "//div[contains(@class,'c-job-description__content')]",
                "//div[contains(@class,'job-description-content')]",
                "//div[contains(@class,'job-description')]",
                "//section[contains(@class,'job-description')]",
                "//div[@id='jobDescription']",
                "//div[@id='job-description']",
                "//div[contains(@class,'description-content')]",
                "//article[contains(@class,'job-detail')]");
        }
        catch { return null; }
    }

    private static DateTime ParseDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DateTime.UtcNow;
        var lower = text.Trim().ToLower();
        if (lower.Contains("bugün") || lower.Contains("saat") || lower.Contains("dakika")) return DateTime.UtcNow;
        if (lower.Contains("dün")) return DateTime.UtcNow.AddDays(-1);
        var m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*gün");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var d)) return DateTime.UtcNow.AddDays(-d);
        m = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*hafta");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var w)) return DateTime.UtcNow.AddDays(-w * 7);
        return DateTime.UtcNow;
    }

    private static string NormalizeJobType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Tam Zamanlı";
        raw = raw.Trim();
        if (raw.Contains("Yarı") || raw.Contains("Part Time")) return "Yarı Zamanlı";
        if (raw.Contains("Staj")) return "Staj";
        if (raw.Contains("Uzaktan")) return "Uzaktan";
        if (raw.Contains("Freelance") || raw.Contains("Serbest")) return "Freelance";
        if (raw.Contains("Dönemsel")) return "Dönemsel";
        return "Tam Zamanlı";
    }
}
