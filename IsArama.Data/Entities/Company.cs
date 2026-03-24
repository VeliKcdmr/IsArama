namespace IsArama.Data.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
