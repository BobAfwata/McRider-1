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

    public string Player1GenderImage => Matchup?.Player1?.Gender?.FirstOrDefault() == 'M' ? "male.png" : "female.png";
    public string Player2GenderImage => Matchup?.Player2?.Gender?.FirstOrDefault() == 'M' ? "male.png" : "female.png";

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

    private bool IsValid(Matchup matchup)
    {
        return matchup != null && matchup.Player1 != null && matchup.Player2 != null;
    }

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
        var matchup = tournament.GetNextMatchup();
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

            if (await _communicator.Initialize() != true)
            {
                IsBusy = false;

                var res = await Application.Current.MainPage.DisplayAlert("Connection failed! Would you like to try again?", "Retry?", "Yes", "No");
                if (res)
                    return await AwaitMatchupsFor(players, game);

                return null;
            }
            IsBusy = false;

            // Navigate to Game Play Page
            await Shell.Current.GoToAsync($"///{nameof(MatchupPage)}");
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
