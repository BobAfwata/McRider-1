using McRider.Common.Services;

namespace McRider.MAUI.Views;

public partial class MatchupUnveilPage : ContentPage
{
    static ILogger logger = App.ServiceProvider.GetService<ILogger<MatchupUnveilPage>>();

    public MatchupUnveilPage(MatchupUnveilPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        CurtainsGrid.SizeChanged += OnCurtainGridSizeChanged;
    }

    private void OnCurtainGridSizeChanged(object? sender, EventArgs e)
    {
        if (BindingContext is MatchupUnveilPageViewModel vm && CurtainsGrid.Width > 0)
        {
            CurtainsGrid.SizeChanged -= OnCurtainGridSizeChanged;
            vm.CurtainWidth = CurtainsGrid.Width;
        }
    }

    protected override void OnAppearing()
    {
        if (BindingContext is MatchupUnveilPageViewModel vm)
            _ = vm.Initialize();

        base.OnAppearing();
    }
}
