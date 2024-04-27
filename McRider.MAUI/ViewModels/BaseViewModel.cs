namespace McRider.MAUI.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    protected ILogger _logger = App.ServiceProvider.GetService<ILogger<BaseViewModel>>();

    public BaseViewModel()
    {
        var type = _logger.GetType().GetGenericTypeDefinition().MakeGenericType(GetType());
        _logger = App.ServiceProvider.GetService(type) as ILogger ?? _logger;

        _logger?.LogInformation($"Created ViewModel: {GetType().Name}");
    }

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _title = "";

    public virtual Task Initialize(params object[] args)
    {
        //_isBusy = false;
        return Task.CompletedTask;
    }

    public virtual Task Uninitialize()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowFlyout()
    {
        _logger.LogInformation("ShowFlyout");

        Shell.Current.ForceLayout();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
        await Task.Delay(100); // Delay for a short period
        Shell.Current.FlyoutIsPresented = true;
        Shell.Current.ForceLayout();
    }

    [RelayCommand]
    private async Task Goback() => await Shell.Current.CurrentPage.Navigation.PopAsync(true);

    [RelayCommand]
    private async Task OpenBrowser(object urlObj)
    {
        _logger?.LogInformation($"Opening url: {urlObj}");

        var url = urlObj?.ToString() ?? "";

        if (url.IsEmail())
            url = "mailto:" + url;
        else if (Regex.IsMatch(url.Replace(" ", "").Replace("-", ""), "\\+?\\d+"))
            url = "tel:" + url.Replace(" ", "").Replace("-", "");

        try
        {
            if (url.StartsWith("mailto:") || url.StartsWith("tel:") || url.IsValidUrl())
                await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.SystemPreferred);
            else
            {
                _logger.LogWarning($"Could not open url '{url}'!");
                //await App.ShowToast($"Could not open url '{url}'!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening url: " + url);
        }
    }
}
