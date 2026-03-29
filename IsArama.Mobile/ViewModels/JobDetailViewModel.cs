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
            HasDescription   = !string.IsNullOrWhiteSpace(Job.Description) || Job.Source == "LinkedIn TR";
            DescriptionHtml  = BuildHtml(Job.Description, Job.OriginalUrl, Job.Source);
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

    private static string BuildHtml(string? content, string? originalUrl = null, string? source = null)
    {
        string body;
        if (!string.IsNullOrWhiteSpace(content))
            body = content;
        else if (source == "LinkedIn TR")
            body = """
                <div style="text-align:center;padding:24px 16px;">
                  <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="#0a66c2" style="margin-bottom:12px;display:block;margin-left:auto;margin-right:auto;">
                    <path d="M19 3a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h14m-.5 15.5v-5.3a3.26 3.26 0 0 0-3.26-3.26c-.85 0-1.84.52-2.32 1.3v-1.11h-2.79v8.37h2.79v-4.93c0-.77.62-1.4 1.39-1.4a1.4 1.4 0 0 1 1.4 1.4v4.93h2.79M6.88 8.56a1.68 1.68 0 0 0 1.68-1.68c0-.93-.75-1.69-1.68-1.69a1.69 1.69 0 0 0-1.69 1.69c0 .93.76 1.68 1.69 1.68m1.39 9.94v-8.37H5.5v8.37h2.77z"/>
                  </svg>
                  <p style="font-weight:600;color:#1e293b;margin-bottom:6px;">LinkedIn ilanları giriş gerektiriyor.</p>
                  <p style="color:#64748b;font-size:13px;">Detayları görmek için "İlana Git" butonuna tıklayın.</p>
                </div>
                """;
        else
            body = "<p style=\"color:#94a3b8;text-align:center;padding:20px;\">Açıklama bulunamadı.<br>Orijinal ilanı ziyaret edin.</p>";

        return $$"""
            <html>
            <head>
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <base href="{{GetBaseUrl(originalUrl)}}">
              <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                html, body { height: auto; margin: 0; padding: 0; }
                #wrap { font-family: -apple-system, 'Segoe UI', sans-serif; font-size: 14px; color: #374151; line-height: 1.7; padding: 12px 4px; }
                p { margin-bottom: 8px; }
                ul, ol { padding-left: 20px; margin-bottom: 10px; }
                li { margin-bottom: 3px; }
                h2, h3, h4 { font-weight: 600; margin-top: 14px; margin-bottom: 6px; font-size: 15px; color: #1e293b; }
                strong, b { font-weight: 600; color: #1e293b; }
                img { max-width: 100%; max-height: 75vh; object-fit: contain; border-radius: 8px; display: block; margin: 8px 0; }
                span { display: inline; }
              </style>
            </head>
            <body><div id="wrap">{{body}}</div></body>
            </html>
            """;
    }

    private static string GetBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "/";
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}/";
        }
        catch { return "/"; }
    }

}
