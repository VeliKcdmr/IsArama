namespace IsArama.Mobile;

public partial class App : Application
{
    public App(AppShell shell)
    {
        InitializeComponent();
        MainPage = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(MainPage!);
}
