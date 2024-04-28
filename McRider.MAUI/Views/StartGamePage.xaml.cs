
using Microsoft.Extensions.DependencyInjection;

namespace McRider.MAUI.Views;

public partial class StartGamePage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<StartGamePage>>();

    public StartGamePage(StartGamePageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public async static Task<List<GamePlay>> GetGamePlays(Player[] players, GameItem game)
    {
        //await Shell.Current.Navigation.PushAsync(new StartGamePage(this));
        await Shell.Current.GoToAsync($"{nameof(StartGamePage)}");

        var vm = App.ServiceProvider.GetService<StartGamePageViewModel>();
        List<GamePlay> gamePlays = null;

        if (vm != null)
            gamePlays = await vm.AwaitGamePlaysFor(players, game);
        else
            _logger?.LogError("StartGamePageViewModel not found");

        await Shell.Current.Navigation.PopAsync();

        return gamePlays;
    }

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
        base.OnAppearing();
    }
}
