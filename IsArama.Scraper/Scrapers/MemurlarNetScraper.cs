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

        foreach (var (slug, jobType) in Categories)
        {
            for (int page = 1; page <= 5; page++)
            {
                try
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

                        var dateText = card.SelectSingleNode(".//text()[normalize-space()!='']")
                                          ?.InnerText?.Trim() ?? "";
                        var city = CityNormalizer.Normalize(title);

                        jobs.Add(new JobDto
                        {
                            Title = "Personel Alımı",
                            CompanyName = title,
                            CompanyLogoUrl = "",
                            City = city,
                            JobType = jobType,
                            OriginalUrl = link.StartsWith("http") ? link
                                           : link.StartsWith("//") ? "https:" + link
                                           : $"https://ilan.memurlar.net{link}",
                            PublishedAt = ParseDate(dateText)
                        });
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{SourceName}] {slug} sayfa {page} atlandı: {ex.Message}");
                    break;
                }
            }
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
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            // Gereksiz bölümleri temizle
            var unwanted = new[]
            {
             "//div[contains(@class,'breadcrumb')]",      // Ana sayfa > Geçici İşçi İlanları
             "//div[contains(@class,'social')]",           // Yorumlar / Abone Ol
             "//a[contains(@class,'print')]",              // Yazdır
             "//a[contains(text(),'Yazdır')]",             // Yazdır (text bazlı)
             "//div[contains(@class,'comment')]",          // Yorum bölümü
             "//div[contains(@class,'yorum')]",
             "//div[contains(@class,'abone')]",
             "//div[contains(@class,'font-size')]",        // Font boyutu butonları (- T T +)
             "//iframe",
            };

            foreach (var xpath in unwanted)
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes != null)
                    foreach (var node in nodes.ToList())
                        node.Remove();
            }

            // <hr> etiketinden sonrasını sil (yorum, sosyal, sidebar vb.)
            var hrNodes = doc.DocumentNode.SelectNodes("//hr");
            if (hrNodes != null)
            {
                foreach (var hr in hrNodes.ToList())
                {
                    var next = hr.NextSibling;
                    while (next != null)
                    {
                        var toRemove = next;
                        next = next.NextSibling;
                        toRemove.Remove();
                    }
                    hr.Remove();
                }
            }

            return DescriptionFetcher.TryXPaths(doc, url,
                "//div[contains(@class,'content-detail panel')]"
                );
        }
        catch { return null; }
    }

    private static DateTime ParseDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DateTime.UtcNow;
        text = text.Trim();

        var months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ocak"] = 1,
            ["Şubat"] = 2,
            ["Mart"] = 3,
            ["Nisan"] = 4,
            ["Mayıs"] = 5,
            ["Haziran"] = 6,
            ["Temmuz"] = 7,
            ["Ağustos"] = 8,
            ["Eylül"] = 9,
            ["Ekim"] = 10,
            ["Kasım"] = 11,
            ["Aralık"] = 12,
        };

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2
            && int.TryParse(parts[0], out int day)
            && months.TryGetValue(parts[1], out int month))
        {
            try
            {
                var year = DateTime.UtcNow.Year;
                if (month > DateTime.UtcNow.Month) year--;
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }
            catch { /* geçersiz tarih, UtcNow döner */ }
        }

        return DateTime.UtcNow;
    }
}
