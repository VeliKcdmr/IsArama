using IsArama.Mobile.ViewModels;

namespace IsArama.Mobile.Pages;

[QueryProperty(nameof(JobId), "JobId")]
public partial class JobDetailPage : ContentPage
{
    private readonly JobDetailViewModel _vm;

    public int JobId { get; set; }

    public JobDetailPage(JobDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _vm.LoadCommand.ExecuteAsync(JobId);
        UpdateWebView();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JobDetailViewModel.DescriptionHtml))
            UpdateWebView();
    }

    private void UpdateWebView()
    {
        if (!string.IsNullOrWhiteSpace(_vm.DescriptionHtml))
        {
            DescriptionWebView.Source = new HtmlWebViewSource
            {
                Html = _vm.DescriptionHtml
            };
        }
    }

    private async void OnDescriptionWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (sender is not WebView webView) return;

        // İçeriğin gerçek yüksekliğini JavaScript ile al
        var result = await webView.EvaluateJavaScriptAsync(
            "Math.max(document.body.scrollHeight, document.documentElement.scrollHeight)");

        if (double.TryParse(result, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var height) && height > 50)
        {
            webView.HeightRequest = height + 20; // küçük padding
        }
    }
}
