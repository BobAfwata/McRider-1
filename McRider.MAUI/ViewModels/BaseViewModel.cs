using System.Dynamic;
using System.Security.Policy;

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

    public ThemeConfig Theme => new ThemeConfig() { Theme = App.Configs?.Theme ?? "showmax" };

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _title = "";

    public virtual Task Initialize(params object[] args)
    {
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

    async Task<ThemeConfig> LoadThemeConfig(string theme = "showmax")
    {
        try
        {
            var filename = $"theme.{theme}.json";

            using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
        }
        catch (Exception e)
        {

        }

        return Theme;
    }
}

public class ThemeTexts
{
    public string TitleColor { get; set; } = null;
    public string GamesTitle { get; set; } = "Lorem ipsum dolor sit amet, consectetuer";
    public string LoadingTitle { get; set; } = "Lorem ipsum dolor sit amet, consectetuer";
    public string LandingTitle { get; set; } = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam";
}

public partial class ThemeConfig : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Logo))]
    [NotifyPropertyChangedFor(nameof(Logo1))]
    [NotifyPropertyChangedFor(nameof(Logo2))]
    [NotifyPropertyChangedFor(nameof(HeaderImage))]
    [NotifyPropertyChangedFor(nameof(FooterImage))]
    [NotifyPropertyChangedFor(nameof(SliderImages))]
    [NotifyPropertyChangedFor(nameof(PrimaryColor))]
    private string _theme = "showmax";

    [ObservableProperty]
    public ThemeTexts _texts;

    public ThemeConfig()
    {
        ReloadTexts();
    }

    partial void OnThemeChanged(string value) => ReloadTexts(value);

    private void ReloadTexts(string theme = null)
    {
        var filename = $"{theme ?? Theme}.texts.json";
        var assembly = Application.Current?.GetType().Assembly;

        var matches = assembly?.GetManifestResourceNames().Where(str => str.EndsWith("." + filename.Replace("/", ".")));        
        if (matches?.Count() > 1)
            throw new Exception($"File name '{filename}' matches multiple resources!!");
        
        if (matches?.Count() == 0)
            return;
        
        using var stream = assembly.GetManifestResourceStream(matches.First());
        using var reader = new StreamReader(stream);

        var contents = reader.ReadToEnd();
        Texts = JsonConvert.DeserializeObject<ThemeTexts>(contents);
    }

    public string TitleColor => Texts.TitleColor ?? TertiaryColor;

    public string PrimaryColor => Theme switch
    {
        "schweppes" => "#FFD700",
        "showmax" => "#FF0055",
        "phillis" => "#0A5Ed8",
        _ => "#0A5Ed8"
    };

    public string TertiaryColor => Theme switch
    {
        "schweppes" => "#000000",
        "showmax" => "#031124",
        "phillis" => "#00000",
        _ => "#00000"
    };

    public ImageSource Logo => GetLogo();

    public ImageSource Logo1 => GetLogo(1);

    public ImageSource Logo2 => GetLogo(2);

    public ImageSource GetLogo(int index = 0) => $"Themes/logo{(index == 0 ? "" : index)}.png".Replace("Themes/", "Themes/" + Theme + "/").ToImageSource();

    public ImageSource ProgressImage => "Themes/progress_back.png".Replace("Themes/", "Themes/" + Theme + "/").ToImageSource("./progress_back.png");

    public ImageSource HeaderImage => "Themes/header.png".Replace("Themes/", "Themes/" + Theme + "/").ToImageSource("./header.png");

    public ImageSource FooterImage => "Themes/footer.png".Replace("Themes/", "Themes/" + Theme + "/").ToImageSource("./footer.png");

    public List<ImageSource> SliderImages => new List<string> { "Themes/slider1.png", "Themes/slider2.png", "Themes/slider3.png" }
        .Select(x => x.Replace("Themes/", "Themes/" + Theme + "/").ToImageSource())
        .Where(x => x != null)
        .ToList();
}