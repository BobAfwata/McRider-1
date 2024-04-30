using McRider.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.ViewModels
{
    public partial class LandingPageViewModel : BaseViewModel
    {
        private RepositoryService<Tournament> _repository;
        private Tournament? _tournament;

        public LandingPageViewModel(RepositoryService<Tournament> repository)
        {
            _repository = repository;
        }

        public override async Task Initialize(params object[] args)
        {
            await base.Initialize(args);
            _tournament = (await _repository.Find(t => t.IsPending == true)).FirstOrDefault();
            OnPropertyChanged(nameof(HasIncompleteTournament));
        }

        public bool HasIncompleteTournament => _tournament?.IsPending == true;

        [RelayCommand]
        async Task ResumeTournament()
        {
            var startGamePage = App.ServiceProvider.GetService<StartGamePage>();
            await Shell.Current.Navigation.PushAsync(startGamePage);

            if (startGamePage?.BindingContext is StartGamePageViewModel vm)
            {
                var players = _tournament.Players.ToArray();
                var tournament = await vm.AwaitMatchupsFor(players, _tournament.Game);
                await tournament.Save();

                // TODO: Show the game results
                //var gameResultsPage = App.ServiceProvider.GetService<GameResultsPage>();
                //await Shell.Current.Navigation.PushAsync(gameResultsPage);
            }
            await Shell.Current.GoToAsync($"///{nameof(GamesPage)}");
        }

        [RelayCommand]
        async Task TakeChallenge()
        {
            await Shell.Current.GoToAsync($"///{nameof(GamesPage)}");
        }
    }
}
