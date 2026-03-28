namespace IsArama.Data.Entities;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string City { get; set; } = null!;
    public string JobType { get; set; } = null!;      // Tam zamanlı, Yarı zamanlı, Uzaktan
    public string OriginalUrl { get; set; } = null!;
    public string Hash { get; set; } = null!;          // Duplikat önleme
    public DateTime PublishedAt { get; set; }
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public int SourceId { get; set; }
    public Source Source { get; set; } = null!;
}
