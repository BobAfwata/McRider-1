using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupRacingPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<MatchupRacingPage>>();

    public MatchupRacingPage(MatchupRacingPageViewModel vm)
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
