namespace IsArama.Data.Entities;

public class Source
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
