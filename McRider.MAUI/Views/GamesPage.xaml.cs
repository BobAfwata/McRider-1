namespace McRider.MAUI.Views;

public partial class GamesPage : ContentPage
{
    public GamesPage(GamesPageViewModel vm)
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