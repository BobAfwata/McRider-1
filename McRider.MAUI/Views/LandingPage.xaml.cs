namespace McRider.MAUI.Views;

public partial class LandingPage : ContentPage
{
    public LandingPage(LandingPageViewModel vm)
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