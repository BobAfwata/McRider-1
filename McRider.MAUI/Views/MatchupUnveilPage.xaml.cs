using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupUnveilPage : ContentPage
{
    static ILogger logger = App.ServiceProvider.GetService<ILogger<MatchupUnveilPage>>();

    public MatchupUnveilPage(MatchupPageViewModel vm)
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
