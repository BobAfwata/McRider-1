namespace McRider.MAUI.Views;

public partial class LoadingPage : ContentPage
{
    FileCacheService _cacheService;

    public LoadingPage(LoadingPageViewModel vm, FileCacheService cacheService)
    {
        InitializeComponent();
        _cacheService = cacheService;
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        // 
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();

        base.OnAppearing();
    }
}