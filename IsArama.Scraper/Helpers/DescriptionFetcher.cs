using HtmlAgilityPack;
using IsArama.Scraper.Dto;

namespace IsArama.Scraper.Helpers;

public static class DescriptionFetcher
{
    /// <summary>
    /// Verilen job listesi için detay sayfalarından description çeker.
    /// maxConcurrent: kaç paralel istek
    /// delayMs: her istek arasındaki min bekleme (ms)
    /// </summary>
    public static async Task FetchAsync(
        List<JobDto> jobs,
        Func<HtmlDocument, string?> extractor,
        string userAgent,
        int maxConcurrent = 4,
        int delayMs = 100)
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(15);
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,*/*;q=0.9");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "tr-TR,tr;q=0.9,en;q=0.8");

        var sem = new SemaphoreSlim(maxConcurrent, maxConcurrent);

        var tasks = jobs.Select(async job =>
        {
            await sem.WaitAsync();
            try
            {
                await Task.Delay(delayMs + Random.Shared.Next(0, 100));
                var html = await http.GetStringAsync(job.OriginalUrl);
                var doc  = new HtmlDocument();
                doc.LoadHtml(html);
                var desc = extractor(doc);
                if (!string.IsNullOrWhiteSpace(desc))
                    job.Description = desc;
            }
            catch { /* ağ/parse hatası — description boş kalır */ }
            finally { sem.Release(); }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Birden fazla XPath dener, içerik bulunan ilkini döner (min 30 karakter).
    /// </summary>    
    public static string? TryXPaths(HtmlDocument doc, string baseUrl, params string[] xpaths)
    {            
        foreach (var xpath in xpaths)
        {
            try
            {
                var node = doc.DocumentNode.SelectSingleNode(xpath);
                if (node == null) continue;
                MakeImagesAbsolute(node, baseUrl);
                var html = node.InnerHtml.Trim();
                if (!string.IsNullOrWhiteSpace(html) && html.Length >= 30)
                    return html;
            }
            catch { }
        }
        return null;
    }
    private static void MakeImagesAbsolute(HtmlNode node, string baseUrl)
    {
        var base_ = new Uri(baseUrl);
        foreach (var img in node.SelectNodes(".//img[@src]") ?? Enumerable.Empty<HtmlNode>())
        {
            var src = img.GetAttributeValue("src", "");
            if (Uri.TryCreate(src, UriKind.Absolute, out _)) continue; // zaten absolute
            var absSrc = new Uri(base_, src).ToString();
            img.SetAttributeValue("src", absSrc);
        }
    }
    /// <summary>
    /// Kariyer.net gibi SPA siteler için __NEXT_DATA__ veya window.__INITIAL_STATE__ JSON içinden
    /// description alanını çıkarır.
    /// </summary>
    public static string? TryNextData(HtmlDocument doc, params string[] jsonKeys)
    {
        var script = doc.DocumentNode
            .SelectSingleNode("//script[@id='__NEXT_DATA__']")?.InnerText
            ?? doc.DocumentNode
               .SelectNodes("//script[not(@src)]")
               ?.Select(s => s.InnerText)
               .FirstOrDefault(t => t?.Contains("\"description\"") == true && t.Length > 200);

        if (string.IsNullOrWhiteSpace(script)) return null;

        foreach (var key in jsonKeys)
        {
            // "key":"VALUE"  veya  "key": "VALUE"
            var pattern = $"\"{key}\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"";
            var m = System.Text.RegularExpressions.Regex.Match(script, pattern);
            if (m.Success)
            {
                var val = System.Text.RegularExpressions.Regex.Unescape(m.Groups[1].Value);
                if (val.Length >= 30) return val;
            }
        }
        return null;
    }
}
