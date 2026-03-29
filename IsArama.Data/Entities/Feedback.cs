namespace IsArama.Data.Entities;

public class Feedback
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Platform { get; set; } // "Web" | "Mobile"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
