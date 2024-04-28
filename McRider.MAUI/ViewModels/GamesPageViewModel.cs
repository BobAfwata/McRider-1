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
            await RegistrationPage.GetPlayers(game);
        }
    }

    private async Task<GameItem[]> GetItemsAsync()
    {
        return
        [
            new GameItem {
                Name = "Single Game",
                PlayersPerTeam = 1,
                TeamsCount = 1,
                Description = "Description 1",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "cycling_single_player.png",
            },
            new GameItem {
                Name = "100M Dash",
                PlayersPerTeam = 1,
                TeamsCount = 2,
                Description = "Description 1",
                TargetDistance = 100,
                TargetTime = TimeSpan.FromMinutes(2),
                Image = "cycling_race_fast.png",
            },
            new GameItem { 
                Name = "P1 vs P2",
                PlayersPerTeam = 1,
                TeamsCount = 2, 
                Description = "Description 2",
                TargetDistance = 1000,
                TargetTime = TimeSpan.FromMinutes(5),
                Image = "cycling_race.png",
            },
            new GameItem {
                Name = "Team up",
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

public class GameItem
{
    public string Image { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int TeamsCount { get; set; }
    public int PlayersPerTeam { get; set; }
    public double? TargetDistance { get; set; }
    public TimeSpan? TargetTime { get; set; }
    public bool AllowLosserToFinish { get; set; } = false;
}