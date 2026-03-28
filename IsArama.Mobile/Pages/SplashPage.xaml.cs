namespace IsArama.Mobile.Pages;

public partial class SplashPage : ContentPage
{
    private int _dotCount = 1;
    private IDispatcherTimer? _timer;
    private readonly Action _onComplete;

    public SplashPage(Action onComplete)
    {
        InitializeComponent();
        _onComplete = onComplete;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += (s, e) =>
        {
            _dotCount = _dotCount >= 3 ? 1 : _dotCount + 1;
            StatusLabel.Text = "İş ilanları alınıyor" + new string('.', _dotCount);
        };
        _timer.Start();

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(2500), () =>
        {
            _timer.Stop();
            _onComplete();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
    }
}
