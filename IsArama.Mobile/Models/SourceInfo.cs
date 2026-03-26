using CommunityToolkit.Mvvm.ComponentModel;

namespace IsArama.Mobile.Models;

public partial class SourceInfo : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public int JobCount { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public string FaviconUrl => $"https://www.google.com/s2/favicons?domain={BaseUrl}&sz=32";
}
