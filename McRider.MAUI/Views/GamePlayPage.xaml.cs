
using McRider.MAUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace McRider.MAUI.Views;

public partial class GamePlayPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<GamePlayPage>>();

    public GamePlayPage(GamePlayPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public static async Task StartGamePlay(GamePlay gamePlay)
    {
        //await Shell.Current.Navigation.PushAsync(new GamePlayPage(this));
        await Shell.Current.GoToAsync($"{nameof(GamePlayPage)}?id={gamePlay.Id}");
        var vm = App.ServiceProvider.GetService<GamePlayPageViewModel>();        
        await vm.StartGamePlay(gamePlay);
        //await Shell.Current.Navigation.PopAsync();
    }

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
        base.OnAppearing();
    }
}
