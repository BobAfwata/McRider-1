using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<MatchupPage>>();

    public MatchupPage(MatchupPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        Player1ProgressImage.SizeChanged += OnCurtainGridSizeChanged;
    }

    private void OnCurtainGridSizeChanged(object? sender, EventArgs e)
    {
        Player1ProgressImage.SizeChanged -= OnCurtainGridSizeChanged;
        if (BindingContext is MatchupPageViewModel vm)
            vm.ProgressFillHeight = Player1ProgressImage.Height;        
    }

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();

        base.OnAppearing();
    }
}
