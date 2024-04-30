using McRider.Common.Helpers;
using McRider.Common.Services;

namespace McRider.MAUI.ViewModels;

public partial class StartGamePageViewModel : BaseViewModel
{
    RepositoryService<Tournament> _repository;
    public StartGamePageViewModel(RepositoryService<Tournament> repository)
    {
        _repository = repository;
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

        _tcs.SetResult();
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
        var teamsArray = players.GroupBy(p => p.Team).ToArray();
        var id = string.Join(",", players.OrderBy(p => p.Id).Select(p => p.Id)).ToMd5();

        var tournament = await _repository.GetAsync(id);
        if (tournament == null || tournament.GetWinner() != null)
            tournament = new Tournament() { Id = id, Players = players.ToList(), Game = game };

        if (tournament.Rounds.Any() != true)
            tournament.Rounds.Add([]);

        var matchups = (tournament.Rounds ??= [[]]).FirstOrDefault();

        // Check if the tournament has started
        if (tournament.Rounds.Sum(r => r.Count) <= 0)
        {
            if (players.Length <= 0)
                throw new InvalidOperationException("At least one player is required to start a game.");
            else if (teamsArray.Count() > 1)
                // Create teamup rounds
                tournament.CreateTeamupRounds(teamsArray);
            else // Create tournament rounds
                tournament.CreateTournamentRounds();

            // Save the tournament
            await tournament.Save();
        }

        var count = 0;
        var matchupCount = tournament.Rounds.Sum(r => r.Count);

        // Play each game
        foreach (var round in tournament.Rounds)
        {
            foreach (var matchup in round)
            {
                // Skip finished games
                if (matchup.GetWinner() != null)
                    continue;

                Matchup = matchup;
                _tcs = new TaskCompletionSource();
                await _tcs.Task;

                // Create a new instance of Game Play Page
                var matchupPage = App.ServiceProvider.GetService<MatchupPage>();

                // Open Game Play Page
                await Shell.Current.Navigation.PushAsync(matchupPage);
                if (matchupPage?.BindingContext is MatchupPageViewModel vm)
                {
                    // Start the game and wait for it to end
                    await vm.StartMatchup(matchup);
                    await tournament.Save();
                }

                // Start next game
                if (++count < matchupCount)
                    await Shell.Current.GoToAsync($"//{nameof(MatchupPage)}");
            }
        }

        return tournament;
    }
}
