namespace IsArama.Mobile;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;

        MainPage = new Pages.SplashPage(() =>
        {
            MainPage = _shell;
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(MainPage!);
}
