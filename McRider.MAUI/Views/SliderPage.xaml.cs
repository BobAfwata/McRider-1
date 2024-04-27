namespace McRider.MAUI.Views;

public partial class SliderPage : ContentPage
{
    public SliderPage(SliderPageViewModel vm)
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