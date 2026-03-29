using IsArama.Mobile.ViewModels;

namespace IsArama.Mobile.Pages;

public partial class FeedbackPage : ContentPage
{
    public FeedbackPage(FeedbackViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
