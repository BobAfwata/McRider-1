using McRider.Common.Services;
using McRider.Domain.Models;

namespace McRider.MAUI.ViewModels;

public partial class StartGamePageViewModel : BaseViewModel
{
    RepositoryService<Tournament> _repository;
    ArdrinoCommunicator _communicator;

    public StartGamePageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
    {
        _repository = repository;
        _communicator = communicator;
    }

    [ObservableProperty]
    Matchup _matchup;

    public bool IsMultiplePlayers => Matchup?.Player2?.IsActive == true;
    public string Player1GenderImage => Matchup?.Player1 == null ? null : Matchup?.Player1?.Gender?.FirstOrDefault() == 'M' ? "male.png" : "female.png";
    public string Player2GenderImage => Matchup?.Player2 == null ? null : Matchup?.Player2?.Gender?.FirstOrDefault() == 'M' ? "male.png" : "female.png";

    [RelayCommand]
    private async Task StartGame()
    {
        if (IsValid(Matchup) != true)
            return;

        _tcs.TrySetResult();
    }

    partial void OnMatchupChanged(Matchup value)
    {
        OnPropertyChanged(nameof(Player1GenderImage));
        OnPropertyChanged(nameof(Player2GenderImage));
    }

    private bool IsValid(Matchup matchup) => matchup?.Players.All(p => p != null) == true;

    TaskCompletionSource _tcs;

    public async Task<Tournament> AwaitMatchupsFor(Player[] players, GameItem game)
    {
        var id = "tournament-" + string.Join(",", players.OrderBy(p => p.Nickname).Select(p => p.Id)).ToMd5();

        var tournament = await _repository.GetAsync(id);
        if (tournament == null || tournament.GetWinner() != null)
            tournament = new Tournament() { Id = id, Players = players.ToList(), Game = game };

        return await AwaitMatchupsFor(tournament, game);
    }

    public async Task<Tournament> AwaitMatchupsFor(Tournament tournament, GameItem game)
    {
        var players = tournament.Players.ToArray();

        var teamsArray = players.GroupBy(p => p.Team).ToArray();
        if (tournament.Rounds.Count == 0)
            tournament.Rounds.Add([]);

        var matchups = (tournament.Rounds ??= [[]]).FirstOrDefault();

        // Check if the tournament has started
        if (tournament.Rounds.Sum(r => r.Count) <= 0)
        {
            if (players.Length <= 0)
                throw new InvalidOperationException("At least one player is required to start a game.");
            else if (game.TeamsCount > 1 || players.Length == 1)
                // Create teamup rounds
                tournament.CreateTeamupRounds(teamsArray);
            else // Create tournament rounds
                tournament.CreateTournamentRounds();

            // Save the tournament
            await tournament.Save();
        }

        // Play each game
        var matchup = tournament.GetNextMatchup() ?? tournament.Matchups.FirstOrDefault();
        while (matchup is not null)
        {
            // Skip finished games
            if (matchup.IsPlayed == true)
            {
                matchup = tournament.GetNextMatchup(matchup);
                continue;
            }

            Matchup = matchup;
            if (_tcs == null || _tcs?.Task?.Status == TaskStatus.RanToCompletion)
                _tcs = new TaskCompletionSource();

            await _tcs.Task;

            IsBusy = true;
            await Task.Delay(1000);

            while (await _communicator.Initialize() != true)
            {
                IsBusy = false;

                // Allow Retry with user confirmation
                var res = await Application.Current.MainPage.DisplayAlert("Connection failed! Would you like to try again?", "Retry?", "Yes", "No");
                if (res != true)
                    return null;

                // Try again
                IsBusy = true;
                await Task.Delay(1000);
            }

            // Start the game
            IsBusy = false;

            // Navigate to Game Play Page
            var matchupPage = tournament.Game.GameType == GameType.Reveal ? nameof(MatchupUnveilPage) : nameof(MatchupPage);
            await Shell.Current.GoToAsync($"///{matchupPage}");
            var vm = App.ServiceProvider.GetService<MatchupPageViewModel>();

            if (vm != null)
            {
                // Start the game and wait for it to end
                await vm.StartMatchup(matchup);
                await tournament.Save();
            }

            matchup = tournament.GetNextMatchup(matchup);
        }

        return tournament;
    }
}
