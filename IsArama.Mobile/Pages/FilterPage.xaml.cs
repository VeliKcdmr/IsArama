using IsArama.Mobile.ViewModels;

namespace IsArama.Mobile.Pages;

public partial class FilterPage : ContentPage
{
    private readonly JobsViewModel _vm;

    public FilterPage(JobsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Temp değerleri aktif filtrelerden doldur
        _vm.TempCity     = _vm.SelectedCity;
        _vm.TempJobType  = _vm.SelectedJobType;
        _vm.TempPosition = _vm.SelectedPosition;
    }
}
