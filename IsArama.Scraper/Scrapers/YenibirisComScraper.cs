using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class YenibirisComScraper : IScraper
{
    public string SourceName => "Yenibiris.com";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        try
        {
            for (int page = 1; page <= 25; page++)
            {
                var url = $"https://www.yenibiris.com/is-ilanlari?page={page}";
                var doc = await web.LoadFromWebAsync(url);

                var cards = doc.DocumentNode.SelectNodes("//div[contains(@class,'listViewRows')]");
                if (cards == null || !cards.Any()) break;

                foreach (var card in cards)
                {
                    // Başlık ve URL
                    var linkNode = card.SelectSingleNode(".//a[@data-ad-id and @href]");
                    var title = linkNode?.GetAttributeValue("title", "")?.Trim();
                    var link  = linkNode?.GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                    title = System.Net.WebUtility.HtmlDecode(title);

                    // Şirket adı
                    var company = System.Net.WebUtility.HtmlDecode(
                        card.SelectSingleNode(".//div[contains(@class,'gtmCompanyName')]")
                            ?.GetAttributeValue("title", "") ?? "Belirtilmemiş");

                    // Şehir — virgüllü çoklu lokasyon varsa ilkini al, " - " ile gelen ilçeyi kaldır
                    var cityRaw = System.Net.WebUtility.HtmlDecode(
                        card.SelectSingleNode(".//div[contains(@class,'gtmLocation')]")
                            ?.GetAttributeValue("title", "") ?? "");
                    var city = CityNormalizer.Normalize(cityRaw);

                    // Logo
                    var logoUrl = card.SelectSingleNode(".//div[contains(@class,'logoWorks')]//img")
                                      ?.GetAttributeValue("src", "") ?? "";

                    // İş tipi — listJobTag span'larından
                    string jobType = "Tam Zamanlı";
                    var tags = card.SelectNodes(".//span[contains(@class,'listJobTag')]");
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            var t = System.Net.WebUtility.HtmlDecode(tag.InnerText.Trim());
                            var normalized = NormalizeJobType(t);
                            if (normalized != "Tam Zamanlı")
                            {
                                jobType = normalized;
                                break;
                            }
                            if (t.Contains("Tam")) jobType = "Tam Zamanlı";
                        }
                    }

                    // Tarih
                    var dateTitle = card.SelectSingleNode(".//*[@title and (contains(@title,'gün') or contains(@title,'Bugün') or contains(@title,'Dün') or contains(@title,'hafta'))]")
                                       ?.GetAttributeValue("title", "") ?? "";

                    jobs.Add(new JobDto
                    {
                        Title          = title,
                        CompanyName    = string.IsNullOrWhiteSpace(company) ? "Belirtilmemiş" : company,
                        CompanyLogoUrl = logoUrl,
                        City           = string.IsNullOrWhiteSpace(city) ? "Belirtilmemiş" : city,
                        JobType        = jobType,
                        OriginalUrl    = link.StartsWith("http") ? link : $"https://www.yenibiris.com{link}",
                        PublishedAt    = ParseDate(dateTitle)
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
                "//div[contains(@class,'jobDescriptionArea')]",
                "//div[contains(@class,'job-description')]",
                "//div[contains(@class,'desc-content')]",
                "//div[contains(@class,'ad-description')]",
                "//div[@id='jobDescription']",
                "//div[@id='job-description']",
                "//section[contains(@class,'description')]",
                "//div[contains(@class,'ilan-icerik')]");
        }
        catch { return null; }
    }

    private static string NormalizeJobType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Tam Zamanlı";
        if (raw.Contains("Yarı") || raw.Contains("Part")) return "Yarı Zamanlı";
        if (raw.Contains("Staj") || raw.Contains("staj")) return "Staj";
        if (raw.Contains("Uzaktan") || raw.Contains("Remote")) return "Uzaktan";
        if (raw.Contains("Freelance") || raw.Contains("Serbest")) return "Freelance";
        if (raw.Contains("Dönemsel")) return "Dönemsel";
        return "Tam Zamanlı";
    }

    private static DateTime ParseDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DateTime.UtcNow;
        text = text.Trim().ToLower();
        if (text.Contains("bugün") || text.Contains("saat") || text.Contains("dakika")) return DateTime.UtcNow;
        if (text.Contains("dün")) return DateTime.UtcNow.AddDays(-1);
        var parts = text.Split(' ');
        if (parts.Length >= 2 && int.TryParse(parts[0], out int n))
        {
            if (parts[1].Contains("gün")) return DateTime.UtcNow.AddDays(-n);
            if (parts[1].Contains("hafta")) return DateTime.UtcNow.AddDays(-n * 7);
        }
        return DateTime.UtcNow;
    }
}
