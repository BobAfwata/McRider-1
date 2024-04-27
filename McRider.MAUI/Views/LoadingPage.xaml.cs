namespace McRider.MAUI.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(LoadingPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
        base.OnAppearing();
    }
}