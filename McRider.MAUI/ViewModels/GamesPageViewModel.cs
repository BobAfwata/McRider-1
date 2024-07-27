namespace McRider.MAUI.ViewModels;

public partial class GamesPageViewModel : BaseViewModel
{
    private FileCacheService _fileCacheService;

    [ObservableProperty]
    private double _columnCount = 2;

    [ObservableProperty]
    private double _rowHeight = 300;

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
        var gameItems = await _fileCacheService.GetAsync(App.Configs?.Theme + ".game-items.json", GetItemsAsync);
        Items = new ObservableCollection<GameItem>(gameItems.Where(g => g.IsActive));
        await base.Initialize(args);
    }

    partial void OnItemsChanged(ObservableCollection<GameItem>? oldValue, ObservableCollection<GameItem> newValue)
    {
        RowHeight = newValue.Count <= 1 ? 600 : 300;
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
        return [
            new GameItem {
                Name = "Reveal",
                GameType = GameType.Reveal,
                PlayersPerTeam = 1,
                TeamsCount = 1,
                Description = "Description 2",
                TargetDistance = 500,
                IsActive = App.Configs?.Theme == "showmax",
                TargetTime = TimeSpan.FromMinutes(1),
                Image = "unveil.png",
            },
            new GameItem {
                Name = "Racing",
                GameType = GameType.Racing,
                PlayersPerTeam = 1,
                TeamsCount = 1,
                Description = "Description 3",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(.75),
                IsActive = App.Configs?.Theme == "shell"
            },
            new GameItem {
                Name = "Tournamet",
                GameType = GameType.Tournament,
                PlayersPerTeam = 16,
                TeamsCount = 1,
                Description = "Description 2",
                TargetDistance = 1000,
                IsActive = true,
                TargetTime = TimeSpan.FromMinutes(1),
                Image = "trophy.png",
            },
            new GameItem {
                Name = "Single Game",
                GameType = GameType.SingleRace,
                PlayersPerTeam = 1,
                TeamsCount = 1,
                Description = "Description 1",
                TargetDistance = 1000,
                IsActive = App.Configs?.Theme == "schweppes",
                TargetTime = TimeSpan.FromMinutes(1),
                Image = "cycling_single_player.png",
            },
            new GameItem {
                Name = "Distance challenge",
                GameType = GameType.SingleRace,
                PlayersPerTeam = 1,
                TeamsCount = 2,
                Description = "Description 2",
                TargetDistance = 1000,
                IsActive  = App.Configs?.Theme == "philips",
                TargetTime = TimeSpan.FromMinutes(1),
                Image = "cycling_race.png",
            },
            new GameItem {
                Name = "Team up",
                GameType = GameType.Team,
                PlayersPerTeam = 2,
                TeamsCount = 2,
                Description = "Description 3",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(1),
                Image = "cycling_team.png",
            }
        ];
    }
}
