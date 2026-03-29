using System.Net.Http.Json;
using System.Text.Json;
using IsArama.Mobile.Models;
using Microsoft.Maui.Storage;
using SourceInfo = IsArama.Mobile.Models.SourceInfo;

namespace IsArama.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };

    public ApiService(HttpClient http) => _http = http;

    public async Task<PagedResult?> GetJobsAsync(
        string? q = null,
        string? city = null,
        string? position = null,
        string? jobType = null,
        string? source = null,
        int page = 1,
        int pageSize = 0)  // 0 = Preferences'tan oku
    {
        bool usePaging = Preferences.Get("UsePagination", true);
        int effectivePageSize = usePaging ? (pageSize > 0 ? pageSize : 20) : 500;
        var url = $"jobs?page={page}&pageSize={effectivePageSize}";
        if (!string.IsNullOrWhiteSpace(q))        url += $"&q={Uri.EscapeDataString(q)}";
        if (!string.IsNullOrWhiteSpace(city))     url += $"&city={Uri.EscapeDataString(city)}";
        if (!string.IsNullOrWhiteSpace(position)) url += $"&position={Uri.EscapeDataString(position)}";
        if (!string.IsNullOrWhiteSpace(jobType))  url += $"&jobType={Uri.EscapeDataString(jobType)}";
        if (!string.IsNullOrWhiteSpace(source))   url += $"&source={Uri.EscapeDataString(source)}";

        try { return await _http.GetFromJsonAsync<PagedResult>(url, Opts); }
        catch { return null; }
    }

    public async Task<JobDetail?> GetJobAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<JobDetail>($"jobs/{id}", Opts); }
        catch { return null; }
    }

    public async Task<List<SourceInfo>> GetSourcesAsync()
    {
        try { return await _http.GetFromJsonAsync<List<SourceInfo>>("sources", Opts) ?? []; }
        catch { return []; }
    }

    public async Task<List<string>> GetCitiesAsync()
    {
        try { return await _http.GetFromJsonAsync<List<string>>("cities", Opts) ?? []; }
        catch { return []; }
    }

    public async Task<List<string>> GetPositionsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<string>>("positions", Opts) ?? []; }
        catch { return []; }
    }

    public async Task<int> GetTotalJobCountAsync()
    {
        var result = await GetJobsAsync(pageSize: 1);
        return result?.Total ?? 0;
    }
}
