using IsArama.Mobile.Models;
using IsArama.Mobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace IsArama.Mobile.Pages;

public partial class JobsPage : ContentPage
{
    private readonly JobsViewModel _vm;

    public JobsPage(JobsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Önce event'leri subscribe et, sonra veri yükle
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm.PropertyChanged += OnVmPropertyChanged;
        _vm.Sources.CollectionChanged -= OnSourcesChanged;
        _vm.Sources.CollectionChanged += OnSourcesChanged;

        if (_vm.Jobs.Count == 0)
            await _vm.InitCommand.ExecuteAsync(null);
    }

    private void OnSourcesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => RebuildSourceChips();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm.Sources.CollectionChanged -= OnSourcesChanged;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(JobsViewModel.SelectedSource) or
                              nameof(JobsViewModel.AllSourceSelected))
            RebuildSourceChips();

    }

    // ── Kaynak chip'lerini programatik olarak oluştur ──
    private void RebuildSourceChips()
    {
        SourceChipsLayout.Children.Clear();

        // "Tümü" chip
        SourceChipsLayout.Children.Add(BuildChip(
            label: "Tümü",
            count: _vm.Sources.Sum(s => s.JobCount),
            faviconUrl: null,
            isActive: _vm.AllSourceSelected,
            onTap: async () =>
            {
                await _vm.SelectAllSourceCommand.ExecuteAsync(null);
                RebuildSourceChips();
            }));

        // Kaynak chip'leri
        foreach (var src in _vm.Sources)
        {
            var srcCopy = src;
            SourceChipsLayout.Children.Add(BuildChip(
                label: srcCopy.Name,
                count: srcCopy.JobCount,
                faviconUrl: srcCopy.FaviconUrl,
                isActive: srcCopy.IsSelected,
                onTap: async () =>
                {
                    await _vm.SelectSourceCommand.ExecuteAsync(srcCopy.Name);
                    RebuildSourceChips();
                }));
        }
    }


    private static View BuildChip(string label, int count, string? faviconUrl, bool isActive, Func<Task> onTap)
    {
        var bg      = isActive ? Color.FromArgb("#2563eb") : Color.FromArgb("#ffffff");
        var stroke  = isActive ? Color.FromArgb("#2563eb") : Color.FromArgb("#e2e8f0");
        var txtClr  = isActive ? Colors.White              : Color.FromArgb("#374151");
        var cntBg   = isActive ? Color.FromArgb("#ffffff33") : Color.FromArgb("#0000001A");

        var stack = new HorizontalStackLayout { Spacing = 5 };

        if (faviconUrl != null)
            stack.Children.Add(new Image
            {
                Source          = ImageSource.FromUri(new Uri(faviconUrl)),
                WidthRequest    = 14,
                HeightRequest   = 14,
                VerticalOptions = LayoutOptions.Center
            });

        stack.Children.Add(new Label
        {
            Text            = label,
            FontSize        = 12,
            FontAttributes  = FontAttributes.Bold,
            TextColor       = txtClr,
            VerticalOptions = LayoutOptions.Center
        });

        var cntBorder = new Border
        {
            BackgroundColor = cntBg,
            StrokeThickness = 0,
            StrokeShape     = new RoundRectangle { CornerRadius = 999 },
            Padding         = new Thickness(5, 1),
            Content         = new Label
            {
                Text           = count.ToString(),
                FontSize       = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor      = txtClr
            }
        };
        stack.Children.Add(cntBorder);

        var border = new Border
        {
            BackgroundColor = bg,
            Stroke          = stroke,
            StrokeThickness = 1.5,
            StrokeShape     = new RoundRectangle { CornerRadius = 999 },
            Padding         = new Thickness(10, 6),
            Content         = stack
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await onTap();
        border.GestureRecognizers.Add(tap);

        return border;
    }
}
