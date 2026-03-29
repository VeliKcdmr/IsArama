using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsArama.Mobile.Services;

namespace IsArama.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly JobsViewModel _jobsVm;

    [ObservableProperty] private bool _usePagination;
    [ObservableProperty] private string _connectionStatus = "Kontrol ediliyor...";
    [ObservableProperty] private Color _connectionColor = Colors.Gray;
    [ObservableProperty] private string _apiUrl = AppConstants.ApiBaseUrl;
    [ObservableProperty] private string _totalJobCount = "—";
    [ObservableProperty] private bool _isChecking;

    public SettingsViewModel(ApiService api, JobsViewModel jobsVm)
    {
        _api = api;
        _jobsVm = jobsVm;
        _usePagination = Preferences.Get("UsePagination", true);
    }

    partial void OnUsePaginationChanged(bool value)
    {
        Preferences.Set("UsePagination", value);
        _jobsVm.UsePagination = value;
        _ = _jobsVm.RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task SendFeedbackAsync()
    {
        var email = "info@isarama.com.tr";
        var subject = Uri.EscapeDataString("İş Arama - Geri Bildirim");
        var uri = new Uri($"mailto:{email}?subject={subject}");
        await Launcher.OpenAsync(uri);
    }

    [RelayCommand]
    public async Task CheckConnectionAsync()
    {
        IsChecking = true;
        ConnectionStatus = "Kontrol ediliyor...";
        ConnectionColor  = Colors.Gray;

        try
        {
            var sources = await _api.GetSourcesAsync();
            var totalJobs = await _api.GetTotalJobCountAsync();
            ConnectionStatus = "Bağlantı başarılı ✓";
            ConnectionColor  = Color.FromArgb("#16a34a");
            TotalJobCount    = $"{totalJobs:N0} ilan";
        }
        catch
        {
            ConnectionStatus = "Bağlantı hatası ✗";
            ConnectionColor  = Color.FromArgb("#dc2626");
            TotalJobCount    = "—";
        }
        finally
        {
            IsChecking = false;
        }
    }
}
