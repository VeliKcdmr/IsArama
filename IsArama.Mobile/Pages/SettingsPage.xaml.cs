using IsArama.Mobile.ViewModels;

namespace IsArama.Mobile.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
