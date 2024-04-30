using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<MatchupPage>>();

    public MatchupPage(MatchupPageViewModel vm)
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
