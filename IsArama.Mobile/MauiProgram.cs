using IsArama.Mobile.Pages;
using IsArama.Mobile.Services;
using IsArama.Mobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace IsArama.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HTTP Client → API
        builder.Services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(AppConstants.ApiBaseUrl),
            Timeout     = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<ApiService>();

        // ViewModels
        builder.Services.AddSingleton<JobsViewModel>();   // singleton → FilterPage ile paylaşım
        builder.Services.AddTransient<JobDetailViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<JobsPage>();
        builder.Services.AddTransient<JobDetailPage>();
        builder.Services.AddTransient<FilterPage>();
        builder.Services.AddTransient<SettingsPage>();

        // AppShell
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
