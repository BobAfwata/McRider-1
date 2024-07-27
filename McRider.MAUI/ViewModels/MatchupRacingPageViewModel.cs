namespace McRider.MAUI.ViewModels;

public partial class MatchupRacingPageViewModel : MatchupPageViewModel
{
    public MatchupRacingPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
        : base(communicator, repository)
    {

    }

    public ImageSource HeaderImage => Theme.GetImage("Themes/race_header.png");
    public ImageSource CityImage => Theme.GetImage("Themes/city.gif");
    public ImageSource RoadImage => Theme.GetImage("Themes/road.gif");
    public ImageSource BusImage => Theme.GetImage("Themes/bus.gif");
}