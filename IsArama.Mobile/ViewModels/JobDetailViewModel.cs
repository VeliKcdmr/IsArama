using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsArama.Mobile.Models;
using IsArama.Mobile.Services;

namespace IsArama.Mobile.ViewModels;

public partial class JobDetailViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private JobDetail? _job;
    [ObservableProperty] private string _descriptionHtml = "";
    [ObservableProperty] private bool _hasDescription;

    public JobDetailViewModel(ApiService api) => _api = api;

    [RelayCommand]
    public async Task LoadAsync(int jobId)
    {
        IsLoading = true;
        Job = await _api.GetJobAsync(jobId);
        if (Job != null)
        {
            HasDescription   = !string.IsNullOrWhiteSpace(Job.Description);
            DescriptionHtml  = BuildHtml(Job.Description);
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task OpenUrlAsync()
    {
        if (Job?.OriginalUrl is { Length: > 0 } url)
            await Launcher.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private static string BuildHtml(string? content)
    {
        var body = string.IsNullOrWhiteSpace(content)
            ? "<p style=\"color:#94a3b8;text-align:center;padding:20px;\">Açıklama bulunamadı.<br>Orijinal ilanı ziyaret edin.</p>"
            : content;

        return $$"""
            <html>
            <head>
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                body { font-family: -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #374151; line-height: 1.7; padding: 12px 0; }
                p { margin-bottom: 8px; }
                div { margin-bottom: 4px; }
                ul, ol { padding-left: 20px; margin-bottom: 10px; }
                li { margin-bottom: 3px; }
                h2, h3, h4 { font-weight: 600; margin-top: 14px; margin-bottom: 6px; font-size: 15px; color: #1e293b; }
                strong, b { font-weight: 600; color: #1e293b; }
                img { max-width: 100%; border-radius: 8px; display: block; margin: 8px 0; }
                span { display: inline; }
              </style>
            </head>
            <body>{{body}}</body>
            </html>
            """;
    }
}
