namespace IsArama.Scraper.Dto;

public class JobDto
{
    public string Title { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string City { get; set; } = null!;
    public string JobType { get; set; } = "Tam Zamanlı";
    public string? Description { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string CategoryName { get; set; } = "Diğer";
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
