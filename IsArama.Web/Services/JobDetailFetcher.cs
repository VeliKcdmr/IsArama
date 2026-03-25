using HtmlAgilityPack;
using IsArama.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Web.Services;

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
            var doc = await web.LoadFromWebAsync(decodedUrl);
            var root = doc.DocumentNode;

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

            return node == null ? null : CleanHtml(node);
        }
        catch
        {
            return null;
        }
    }

    private static HtmlNode? GetIsbulNode(HtmlNode root)
    {
        // İçerik SSR HTML'de ul.list-node + p.text-node olarak geliyor
        var container = root.SelectSingleNode(
            "//div[contains(@class,'overflow-hidden') and contains(@class,'break-words')]");
        if (container != null) return container;

        // Fallback: JSON-LD description alanından metin olarak al
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

        // Yalnızca içerik elementlerini tut (span başlıklar + p/ul/li/strong)
        // div'ler = aksiyonlar (İşe Başvur, veya, Cv Oluştur, İlanı Şikayet Et) — hepsini kaldır
        var divs = node.SelectNodes(".//div");
        if (divs != null)
            foreach (var d in divs.ToList())
                d.Remove();

        // Yinelenen H2 başlığı kaldır (şirket adı - zaten modalda gösteriliyor)
        var h2 = node.SelectNodes(".//h2[contains(@class,'cj-title')]");
        if (h2 != null)
            foreach (var h in h2.ToList())
                h.Remove();

        return node;
    }

    private static HtmlNode? GetMemurlarNode(HtmlNode root)
    {
        const string baseUrl = "https://ilan.memurlar.net";
        var sb = new System.Text.StringBuilder();

        // Metin içerik (div.detail) — eski ilanlar metin, yeni ilanlar resim içerebilir
        var detail = root.SelectSingleNode("//article//div[contains(@class,'detail')]");
        if (detail != null)
        {
            // Script ve iframe'leri temizle
            var toRemove = detail.SelectNodes(".//script | .//iframe | .//ins");
            if (toRemove != null)
                foreach (var n in toRemove.ToList())
                    n.Remove();

            // İçerideki görsellerin URL'lerini mutlak adrese çevir
            var detailImgs = detail.SelectNodes(".//img[@src]");
            if (detailImgs != null)
                foreach (var img in detailImgs)
                {
                    var src = img.GetAttributeValue("src", "");
                    src = AbsoluteUrl(src, baseUrl);
                    img.SetAttributeValue("src", src);
                    img.SetAttributeValue("style", "max-width:100%;display:block;margin:8px 0;border-radius:6px;");
                    // Diğer attribute'ları temizle
                    foreach (var a in img.Attributes.Where(x => x.Name != "src" && x.Name != "alt" && x.Name != "style").Select(x => x.Name).ToList())
                        img.Attributes.Remove(a);
                }

            StripAttributes(detail);
            sb.Append(detail.InnerHtml.Trim());
        }

        // Doküman görselleri (taranan belge olarak gelen ilanlar)
        var docImgs = root.SelectNodes("//article//img[@src]");
        if (docImgs != null)
        {
            foreach (var img in docImgs)
            {
                var src = img.GetAttributeValue("src", "");
                if (!src.Contains("/common/job/advert/documents/")) continue;
                src = AbsoluteUrl(src, baseUrl);
                var alt = img.GetAttributeValue("alt", "");
                sb.Append($"<img src=\"{src}\" alt=\"{alt}\" style=\"max-width:100%;display:block;margin-top:12px;border-radius:8px;\"/>");
            }
        }

        if (sb.Length == 0) return null;
        return HtmlNode.CreateNode($"<div>{sb}</div>");
    }

    private static string AbsoluteUrl(string src, string baseUrl)
    {
        if (src.StartsWith("http")) return src;
        if (src.StartsWith("//"))   return "https:" + src;
        return baseUrl + (src.StartsWith("/") ? src : "/" + src);
    }

    private static string CleanHtml(HtmlNode node)
    {
        var removeTags = new[] { "script", "style", "nav", "form", "button", "input", "select", "textarea", "header", "footer", "aside" };
        foreach (var tag in removeTags)
        {
            var nodes = node.SelectNodes($".//{tag}");
            if (nodes != null)
                foreach (var n in nodes.ToList())
                    n.Remove();
        }

        StripAttributes(node);
        return node.InnerHtml.Trim();
    }

    private static void StripAttributes(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Element)
        {
            bool isImg = node.Name == "img";
            var toRemove = node.Attributes
                .Where(a => !(isImg && (a.Name == "src" || a.Name == "alt" || a.Name == "style")))
                .Select(a => a.Name)
                .ToList();
            foreach (var a in toRemove) node.Attributes.Remove(a);
        }
        foreach (var child in node.ChildNodes.ToList())
            StripAttributes(child);
    }
}
