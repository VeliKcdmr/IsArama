using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using IsArama.Scraper.Dto;
using IsArama.Scraper.Helpers;
using IsArama.Scraper.Interfaces;

namespace IsArama.Scraper.Scrapers;

public class KariyerNetScraper : IScraper
{
    public string SourceName => "Kariyer.net";

    private const string ApiUrl = "https://candidatesearchapigateway.kariyer.net/search";
    private const string DefaultLogo = "firma-logosuz";

    public async Task<List<JobDto>> ScrapeAsync()
    {
        var jobs = new List<JobDto>();

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        var searchUrls = new[]
        {
            (Url: ApiUrl,                                                                              Pages: 25),
            (Url: "https://candidatesearchapigateway.kariyer.net/search?calistirma-sekli=yari-zamanli", Pages: 25),
            (Url: "https://candidatesearchapigateway.kariyer.net/search?calistirma-sekli=staj",         Pages: 25),
            (Url: "https://candidatesearchapigateway.kariyer.net/search?calistirma-sekli=uzaktan",      Pages: 25),
        };

        try
        {
            foreach (var (url, maxPages) in searchUrls)
                for (int page = 1; page <= maxPages; page++)
                {
                    var payload = new { page, pageSize = 25 };
                    var response = await http.PostAsJsonAsync(url, payload);
                    if (!response.IsSuccessStatusCode) break;

                    var json = await response.Content.ReadFromJsonAsync<KariyerResponse>();
                    var items = json?.Data?.Jobs?.Items;
                    if (items == null || items.Count == 0) break;

                    foreach (var item in items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.JobUrl))
                            continue;

                        var logoUrl = item.SquareLogoUrl ?? "";
                        if (string.IsNullOrWhiteSpace(logoUrl) && !string.IsNullOrWhiteSpace(item.LogoUrl))
                            logoUrl = $"https://img-kariyer.mncdn.com/UploadFiles/Clients/Logolar/{item.LogoUrl}";

                        var city = CityNormalizer.Normalize(item.LocationText ?? item.AllLocations);

                        jobs.Add(new JobDto
                        {
                            Title = item.Title,
                            CompanyName = string.IsNullOrWhiteSpace(item.CompanyName) ? "Belirtilmemiş" : item.CompanyName,
                            CompanyLogoUrl = logoUrl,
                            City = city,
                            JobType = NormalizeJobType(item.WorkTypeText ?? item.WorkType ?? ""),
                            OriginalUrl = item.JobUrl.StartsWith("http") ? item.JobUrl : $"https://www.kariyer.net{item.JobUrl}",
                            PublishedAt = DateTime.TryParse(item.PostingDate, out var dt) ? dt : DateTime.UtcNow
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
        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(15);
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ua);
        try
        {
            var html = await http.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return DescriptionFetcher.TryXPaths(doc, url,
            "//div[contains(@class,'job-detail-container-description')]");

        }
        catch { return null; }
    }

    private static string NormalizeJobType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Tam Zamanlı";
        if (raw.Contains("PartTime") || raw.Contains("Yarı") || raw.Contains("Part Time")) return "Yarı Zamanlı";
        if (raw.Contains("Internship") || raw.Contains("Staj")) return "Staj";
        if (raw.Contains("Remote") || raw.Contains("Uzaktan")) return "Uzaktan";
        if (raw.Contains("Freelance") || raw.Contains("Serbest")) return "Freelance";
        if (raw.Contains("Seasonal") || raw.Contains("Dönemsel")) return "Dönemsel";
        return "Tam Zamanlı";
    }

    private class KariyerResponse
    {
        [JsonPropertyName("data")] public KariyerData? Data { get; set; }
    }
    private class KariyerData
    {
        [JsonPropertyName("jobs")] public KariyerJobs? Jobs { get; set; }
    }
    private class KariyerJobs
    {
        [JsonPropertyName("items")] public List<KariyerJobItem>? Items { get; set; }
    }
    private class KariyerJobItem
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
        [JsonPropertyName("jobUrl")] public string? JobUrl { get; set; }
        [JsonPropertyName("logoUrl")] public string? LogoUrl { get; set; }
        [JsonPropertyName("squareLogoUrl")] public string? SquareLogoUrl { get; set; }
        [JsonPropertyName("locationText")] public string? LocationText { get; set; }
        [JsonPropertyName("allLocations")] public string? AllLocations { get; set; }
        [JsonPropertyName("workType")] public string? WorkType { get; set; }
        [JsonPropertyName("workTypeText")] public string? WorkTypeText { get; set; }
        [JsonPropertyName("postingDate")] public string? PostingDate { get; set; }
    }
}
