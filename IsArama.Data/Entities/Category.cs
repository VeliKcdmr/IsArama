namespace IsArama.Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
