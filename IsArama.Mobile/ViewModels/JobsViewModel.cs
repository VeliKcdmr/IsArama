using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsArama.Mobile.Models;
using IsArama.Mobile.Services;
using Microsoft.Maui.Storage;
using SourceInfo = IsArama.Mobile.Models.SourceInfo;

namespace IsArama.Mobile.ViewModels;

public partial class JobsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private int _currentPage = 1;
    private bool _hasMore = true;
    private bool _isLoadingMore;

    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _isFilterOpen;
    [ObservableProperty] private bool _allSourceSelected = true;
    [ObservableProperty] private bool _isLoadingMoreVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultCountText))]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(HasPrevPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int _totalResults;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(HasPrevPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private bool _usePagination;

    // Aktif filtreler
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string? _selectedSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string? _selectedCity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string? _selectedJobType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    [NotifyPropertyChangedFor(nameof(ActiveFilterSummary))]
    private string? _selectedPosition;

    // Geçici filtre değerleri (sheet açıkken düzenleniyor)
    [ObservableProperty] private string? _tempCity;
    [ObservableProperty] private string? _tempJobType;
    [ObservableProperty] private string? _tempPosition;

    public ObservableCollection<JobListItem> Jobs { get; } = [];
    public ObservableCollection<SourceInfo> Sources { get; } = [];
    public ObservableCollection<string> Cities { get; } = [];
    public ObservableCollection<string> JobTypes { get; } =
        ["Tam Zamanlı", "Sözleşmeli", "Memur", "Staj", "Yarı Zamanlı"];

    public bool HasActiveFilters =>
        SelectedCity != null || SelectedJobType != null || SelectedPosition != null;

    public string ResultCountText =>
        TotalResults > 0 ? $"{TotalResults:N0} ilan bulundu" : "Sonuç bulunamadı";

    public string ActiveFilterSummary
    {
        get
        {
            var parts = new List<string>();
            if (SelectedCity != null)     parts.Add($"📍 {SelectedCity}");
            if (SelectedJobType != null)  parts.Add($"⏱ {SelectedJobType}");
            if (SelectedPosition != null) parts.Add($"💼 {SelectedPosition}");
            return parts.Count > 0 ? string.Join("  •  ", parts) : "";
        }
    }

    public int TotalPages => _usePagination && TotalResults > 0
        ? (int)Math.Ceiling(TotalResults / 20.0) : 1;
    public bool HasNextPage => _usePagination && _currentPage < TotalPages;
    public bool HasPrevPage => _usePagination && _currentPage > 1;
    public string PageInfo => _usePagination ? $"Sayfa {_currentPage} / {TotalPages}" : "";

    public JobsViewModel(ApiService api)
    {
        _api = api;
        _usePagination = Preferences.Get("UsePagination", true);
    }

    partial void OnSelectedSourceChanged(string? value)
    {
        AllSourceSelected = value == null;
        foreach (var s in Sources)
            s.IsSelected = s.Name == value;
    }

    [RelayCommand]
    public async Task InitAsync()
    {
        var sourcesTask = _api.GetSourcesAsync();
        var citiesTask  = _api.GetCitiesAsync();
        await Task.WhenAll(sourcesTask, citiesTask);

        Sources.Clear();
        foreach (var s in sourcesTask.Result)
            Sources.Add(s);

        Cities.Clear();
        foreach (var c in citiesTask.Result.Where(x => x != "Belirtilmemiş").Take(60))
            Cities.Add(c);

        await ReloadAsync();
    }

    [RelayCommand]
    public async Task SelectSourceAsync(string? sourceName)
    {
        SelectedSource = SelectedSource == sourceName ? null : sourceName;
        await ReloadAsync();
    }

    [RelayCommand]
    public async Task SelectAllSourceAsync()
    {
        SelectedSource = null;
        await ReloadAsync();
    }

    [RelayCommand]
    public async Task SearchAsync() => await ReloadAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        await ReloadAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task LoadMoreAsync()
    {
        if (UsePagination || _isLoadingMore || !_hasMore) return;
        _isLoadingMore = true;
        IsLoadingMoreVisible = true;
        await LoadPageAsync(_currentPage + 1, append: true);
        _isLoadingMore = false;
        IsLoadingMoreVisible = false;
    }

    [RelayCommand]
    public async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        await LoadPageAsync(_currentPage + 1, append: false);
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(HasPrevPage));
        OnPropertyChanged(nameof(PageInfo));
    }

    [RelayCommand]
    public async Task PrevPageAsync()
    {
        if (!HasPrevPage) return;
        await LoadPageAsync(_currentPage - 1, append: false);
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(HasPrevPage));
        OnPropertyChanged(nameof(PageInfo));
    }

    [RelayCommand]
    public void OpenFilter()
    {
        TempCity     = SelectedCity;
        TempJobType  = SelectedJobType;
        TempPosition = SelectedPosition;
        IsFilterOpen = true;
    }

    [RelayCommand]
    public void CloseFilter() => IsFilterOpen = false;

    [RelayCommand]
    public async Task ApplyFilterAsync()
    {
        SelectedCity     = TempCity;
        SelectedJobType  = TempJobType;
        SelectedPosition = TempPosition;
        IsFilterOpen     = false;
        OnPropertyChanged(nameof(ActiveFilterSummary));
        await ReloadAsync();
        // Filtre sayfasından uygulayınca İlanlar tabına geç
        if (Shell.Current.CurrentPage is not Pages.JobsPage)
            await Shell.Current.GoToAsync("//JobsPage");
    }

    [RelayCommand]
    public async Task ClearFilterAsync()
    {
        TempCity         = null;
        TempJobType      = null;
        TempPosition     = null;
        SelectedCity     = null;
        SelectedJobType  = null;
        SelectedPosition = null;
        IsFilterOpen     = false;
        OnPropertyChanged(nameof(ActiveFilterSummary));
        await ReloadAsync();
        if (Shell.Current.CurrentPage is not Pages.JobsPage)
            await Shell.Current.GoToAsync("//JobsPage");
    }

    [RelayCommand]
    public async Task NavigateToDetailAsync(int jobId) =>
        await Shell.Current.GoToAsync(nameof(Pages.JobDetailPage),
            new Dictionary<string, object> { ["JobId"] = jobId });

    private async Task ReloadAsync()
    {
        _currentPage = 1;
        _hasMore     = true;
        await LoadPageAsync(1, append: false);
    }

    private async Task LoadPageAsync(int page, bool append)
    {
        if (!append) IsLoading = true;

        var result = await _api.GetJobsAsync(
            q:        SearchQuery,
            city:     SelectedCity,
            position: SelectedPosition,
            jobType:  SelectedJobType,
            source:   SelectedSource,
            page:     page);

        if (!append) { Jobs.Clear(); IsLoading = false; }

        if (result == null) return;

        foreach (var j in result.Jobs)
            Jobs.Add(j);

        TotalResults = result.Total;
        _currentPage = page;
        _hasMore     = result.Jobs.Count == result.PageSize;
    }
}
