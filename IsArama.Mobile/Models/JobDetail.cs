namespace IsArama.Mobile.Models;

public class JobDetail
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string City { get; set; } = "";
    public string JobType { get; set; } = "";
    public string OriginalUrl { get; set; } = "";
    public DateTime PublishedAt { get; set; }
    public string Company { get; set; } = "";
    public string? CompanyLogo { get; set; }
    public string Category { get; set; } = "";
    public string Source { get; set; } = "";
    public string SourceUrl { get; set; } = "";

    public string Initials => Company.Length > 0 ? Company[..1].ToUpperInvariant() : "?";
    public bool HasLogo => !string.IsNullOrWhiteSpace(CompanyLogo);

    public Color LogoColor
    {
        get
        {
            if (Company.Length == 0) return Color.FromArgb("#f1f5f9");
            var hue = (Company[0] * 137 % 360) / 360f;
            return Color.FromHsva(hue, 0.25f, 0.95f, 1f);
        }
    }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - PublishedAt;
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dk önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
            if (diff.TotalDays < 2) return "Dün";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} hafta önce";
            return PublishedAt.ToString("dd MMM yyyy");
        }
    }
}
