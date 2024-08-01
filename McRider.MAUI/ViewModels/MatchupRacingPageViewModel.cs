namespace McRider.MAUI.ViewModels;

public partial class MatchupRacingPageViewModel : MatchupPageViewModel
{
    [ObservableProperty]
    public ImageSource _bubbleImage;

    public MatchupRacingPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
        : base(communicator, repository)
    {
        BubbleImage = Theme.GetImage($"Themes/bubble_0.png");
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(PercentageTimeProgress) && PercentageTimeProgress > 0)
        {
            var bubbleCount = 8;
            var bubbleIndex = (int)Math.Ceiling(PercentageTimeProgress * (bubbleCount - 1) / 100);
            BubbleImage = Theme.GetImage($"Themes/bubble_{bubbleIndex}.png");
        }
    }

    public ImageSource HeaderImage => Theme.GetImage("Themes/race_header.png");
    public ImageSource CityImage => Theme.GetImage("Themes/city.gif");
    public ImageSource RoadImage => Theme.GetImage("Themes/road.gif");
    public ImageSource BusImage => Theme.GetImage("Themes/bus.gif");

}