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

        try
        {
            for (int page = 1; page <= 15; page++)
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

                    jobs.Add(new JobDto
                    {
                        Title          = title,
                        CompanyName    = company,
                        CompanyLogoUrl = logoUrl ?? "",
                        City           = city,
                        JobType        = jobType,
                        OriginalUrl    = link.StartsWith("http") ? link : $"https://www.eleman.net{link}",
                        PublishedAt    = DateTime.UtcNow
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
