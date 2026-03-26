namespace IsArama.Mobile.Models;

public class CategoryInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public override string ToString() => Name;
}
