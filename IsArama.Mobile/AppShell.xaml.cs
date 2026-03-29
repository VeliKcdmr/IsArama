using IsArama.Mobile.Pages;

namespace IsArama.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(JobDetailPage), typeof(JobDetailPage));
        Routing.RegisterRoute(nameof(FeedbackPage), typeof(FeedbackPage));
    }
}
