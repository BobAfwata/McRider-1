using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupPage : ContentPage
{
    static ILogger _logger = App.ServiceProvider.GetService<ILogger<MatchupPage>>();

    public MatchupPage(MatchupPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        Player1ProgressImage.SizeChanged += OnPlayer1ProgressImageChanged;
    }

    private void OnPlayer1ProgressImageChanged(object? sender, EventArgs e)
    {
        if (BindingContext is MatchupPageViewModel vm)
        {
            if (Player1ProgressImage.Height > 0)
            {
                Player1ProgressImage.SizeChanged -= OnPlayer1ProgressImageChanged;
                vm.ProgressFillHeight = Player1ProgressImage.Height;
            }
        }
    }

    protected override void OnAppearing()
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();

        base.OnAppearing();
    }
}
