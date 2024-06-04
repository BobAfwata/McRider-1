using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupUnveilPage : ContentPage
{
    static ILogger logger = App.ServiceProvider.GetService<ILogger<MatchupUnveilPage>>();

    public MatchupUnveilPage(MatchupPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        CurtainsGrid.SizeChanged += OnCurtainGridSizeChanged;
    }

    private void OnCurtainGridSizeChanged(object? sender, EventArgs e)
    {
        CurtainsGrid.SizeChanged -= OnCurtainGridSizeChanged;
        if (BindingContext is MatchupPageViewModel vm)
            vm.CurtainWidth = CurtainsGrid.Width;
    }

    protected override void OnAppearing()
    {
        if (BindingContext is MatchupPageViewModel vm)
            _ = vm.Initialize();

        base.OnAppearing();
    }
}
