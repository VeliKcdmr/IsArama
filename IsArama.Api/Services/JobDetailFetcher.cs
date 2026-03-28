using HtmlAgilityPack;
using IsArama.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Services;

public class JobDetailFetcher
{
    private readonly ApplicationDbContext _db;

    public JobDetailFetcher(ApplicationDbContext db) => _db = db;

    public async Task<string?> GetOrFetchAsync(int jobId)
    {
        var job = await _db.Jobs
            .Include(j => j.Source)
            .FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null) return null;

        if (!string.IsNullOrWhiteSpace(job.Description))
            return job.Description;

        var html = await FetchAsync(job.OriginalUrl, job.Source.Name);
        if (!string.IsNullOrWhiteSpace(html))
        {
            job.Description = html;
            await _db.SaveChangesAsync();
        }
        return html;
    }

    private static async Task<string?> FetchAsync(string url, string sourceName)
    {
        if (sourceName == "LinkedIn TR") return null;

        try
        {
            var decodedUrl = System.Net.WebUtility.HtmlDecode(url);
            var web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
            var doc  = await web.LoadFromWebAsync(decodedUrl);
            var root = doc.DocumentNode;

            var baseUrl = sourceName switch
            {
                "Kariyer.net"   => "https://www.kariyer.net",
                "Eleman.net"    => "https://www.eleman.net",
                "Yenibiris.com" => "https://www.yenibiris.com",
                "Secretcv.com"  => "https://www.secretcv.com",
                "Memurlar.net"  => "https://ilan.memurlar.net",
                "İşbul.net"     => "https://www.isbul.net",
                _               => ""
            };

            HtmlNode? node = sourceName switch
            {
                "Kariyer.net"   => root.SelectSingleNode("//div[contains(@class,'job-detail-container-description')]"),
                "Eleman.net"    => root.SelectSingleNode("//div[contains(@class,'d-information')]"),
                "Yenibiris.com" => root.SelectSingleNode("//div[contains(@class,'mainInfos')]"),
                "Secretcv.com"  => GetSecretcvNode(root),
                "Memurlar.net"  => GetMemurlarNode(root),
                "İşbul.net"     => GetIsbulNode(root),
                _               => null
            };

            return node == null ? null : CleanHtml(node, baseUrl);
        }
        catch
        {
            return null;
        }
    }

    private static HtmlNode? GetIsbulNode(HtmlNode root)
    {
        var container = root.SelectSingleNode(
            "//div[contains(@class,'overflow-hidden') and contains(@class,'break-words')]");
        if (container != null) return container;

        var scripts = root.SelectNodes("//script[@type='application/ld+json']");
        if (scripts == null) return null;
        foreach (var script in scripts)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(script.InnerText);
                if (!doc.RootElement.TryGetProperty("description", out var descEl)) continue;
                var text = descEl.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(text)) continue;
                var escaped = System.Net.WebUtility.HtmlEncode(text);
                return HtmlNode.CreateNode($"<p>{escaped}</p>");
            }
            catch { }
        }
        return null;
    }

    private static HtmlNode? GetSecretcvNode(HtmlNode root)
    {
        var nodes = root.SelectNodes("//div[contains(@class,'content-job')]");
        if (nodes == null || nodes.Count < 2) return null;
        var node = nodes[1];

        var divs = node.SelectNodes(".//div");
        if (divs != null)
            foreach (var d in divs.ToList())
                d.Remove();

        var h2 = node.SelectNodes(".//h2[contains(@class,'cj-title')]");
        if (h2 != null)
            foreach (var h in h2.ToList())
                h.Remove();

        return node;
    }

    private static HtmlNode? GetMemurlarNode(HtmlNode root)
    {
        const string baseUrl = "https://ilan.memurlar.net";

        var unwanted = new[]
        {
        "//div[contains(@class,'breadcrumb')]",
        "//div[contains(@class,'social')]",
        "//a[contains(@class,'print')]",
        "//a[contains(text(),'Yazdır')]",
        "//div[contains(@class,'comment')]",
        "//div[contains(@class,'yorum')]",
        "//div[contains(@class,'abone')]",
        "//div[contains(@class,'font-size')]",
    };

        foreach (var xpath in unwanted)
        {
            var nodes = root.SelectNodes(xpath);
            if (nodes != null)
                foreach (var n in nodes.ToList())
                    n.Remove();
        }

        var node = root.SelectSingleNode("//div[contains(@class,'content-detail panel')]");
        if (node == null) return null;

        var imgs = node.SelectNodes(".//img[@src]");
        if (imgs != null)
            foreach (var img in imgs)
            {
                var src = AbsoluteUrl(img.GetAttributeValue("src", ""), baseUrl);
                img.SetAttributeValue("src", src);
            }

        return node;
    }


    private static string AbsoluteUrl(string src, string baseUrl)
    {
        if (src.StartsWith("http")) return src;
        if (src.StartsWith("//"))   return "https:" + src;
        return baseUrl + (src.StartsWith("/") ? src : "/" + src);
    }

    private static string CleanHtml(HtmlNode node, string baseUrl = "")
    {
        var removeTags = new[] { "script", "style", "nav", "form", "button", "input", "select", "textarea", "header", "footer", "aside", "iframe" };
        foreach (var tag in removeTags)
        {
            var nodes = node.SelectNodes($".//{tag}");
            if (nodes != null)
                foreach (var n in nodes.ToList())
                    n.Remove();
        }

        // Lazy-load: data-src → src, relative URL → absolute
        var imgs = node.SelectNodes(".//img");
        if (imgs != null)
        {
            foreach (var img in imgs)
            {
                var src     = img.GetAttributeValue("src", "");
                var dataSrc = img.GetAttributeValue("data-src", "");
                var lazySrc = img.GetAttributeValue("data-lazy-src", "");

                var best = !string.IsNullOrWhiteSpace(dataSrc) ? dataSrc
                         : !string.IsNullOrWhiteSpace(lazySrc)  ? lazySrc
                         : src;

                if (!string.IsNullOrWhiteSpace(best) && !string.IsNullOrWhiteSpace(baseUrl))
                    best = AbsoluteUrl(best, baseUrl);

                img.Attributes.RemoveAll();
                if (!string.IsNullOrWhiteSpace(best))
                    img.SetAttributeValue("src", best);
                img.SetAttributeValue("style", "max-width:100%;display:block;margin:6px 0;border-radius:6px;");
            }
        }

        StripAttributes(node);
        return node.InnerHtml.Trim();
    }

    private static void StripAttributes(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Element && node.Name != "img")
        {
            var toRemove = node.Attributes.Select(a => a.Name).ToList();
            foreach (var a in toRemove) node.Attributes.Remove(a);
        }
        foreach (var child in node.ChildNodes.ToList())
            StripAttributes(child);
    }
}
