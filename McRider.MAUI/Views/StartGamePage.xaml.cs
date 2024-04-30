
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

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
        base.OnAppearing();
    }
}
