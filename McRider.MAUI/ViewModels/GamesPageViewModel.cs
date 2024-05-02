using McRider.Common.Services;

namespace McRider.MAUI.ViewModels;

public partial class GamesPageViewModel : BaseViewModel
{
    private FileCacheService _fileCacheService;

    [ObservableProperty]
    private int _columnCount = 2;

    [ObservableProperty]
    private GameItem _selectedItem;

    [ObservableProperty]
    private ObservableCollection<GameItem> _items = [];

    public GamesPageViewModel(FileCacheService fileCacheService)
    {
        _fileCacheService = fileCacheService;
    }

    override public async Task Initialize(params object[] args)
    {
        Items = new ObservableCollection<GameItem>(await _fileCacheService.GetAsync("game-items.json", GetItemsAsync));
        await base.Initialize(args);
    }

    partial void OnItemsChanged(ObservableCollection<GameItem>? oldValue, ObservableCollection<GameItem> newValue)
    {
        ColumnCount = (int)Math.Max(1, Math.Min(3, Math.Ceiling(Math.Sqrt(newValue.Count))));
    }

    [RelayCommand]
    private async Task ItemClicked(object args)
    {
        if (args is GameItem game)
        {
            var registrationPage = App.ServiceProvider.GetService<RegistrationPage>();

            await Shell.Current.Navigation.PushAsync(registrationPage);
            if (registrationPage?.BindingContext is RegistrationPageViewModel vm)
            {
                var players = await vm.AwaitPlayersFor(game);
                await players.SaveAll();
            }
        }
    }

    private async Task<GameItem[]> GetItemsAsync()
    {
        return
        [
            new GameItem {
                Name = "Single Game",
                GameType = GameType.SingleRace,
                PlayersPerTeam = 1,
                TeamsCount = 1,
                Description = "Description 1",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "cycling_single_player.png",
            },
            new GameItem {
                Name = "Tournamet",
                GameType = GameType.Tournament,
                PlayersPerTeam = 16,
                TeamsCount = 1,
                Description = "Description 2",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "trophy.png",
            },
            new GameItem { 
                Name = "P1 vs P2",
                GameType = GameType.SingleRace,
                PlayersPerTeam = 1,
                TeamsCount = 2, 
                Description = "Description 2",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "cycling_race.png",
            },
            new GameItem {
                Name = "Team up",
                GameType = GameType.Team,
                PlayersPerTeam = 2,
                TeamsCount = 2,
                Description = "Description 3",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "cycling_team.png",
            },
        ];
    }
}
