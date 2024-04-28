
using Microsoft.Extensions.DependencyInjection;

namespace McRider.MAUI.Views;

public partial class RegistrationPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<RegistrationPage>>();

    public RegistrationPage(RegistrationPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public async static Task<List<Player>> GetPlayers(GameItem game)
    {
        var vm = App.ServiceProvider.GetService<RegistrationPageViewModel>();
        if (vm != null)
            return await vm.AwaitPlayersFor(game);

        _logger?.LogError("RegistrationPageViewModel not found");
        return null;
    }

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
        base.OnAppearing();
    }
}