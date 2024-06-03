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
            repository.FilePrefix = App.Configs?.Theme + ".";
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
            await Shell.Current.GoToAsync($"///{nameof(StartGamePage)}");
            var vm = App.ServiceProvider.GetService<StartGamePageViewModel>();

            if (vm is not null)
            {
                var tournament = await vm.AwaitMatchupsFor(_tournament, _tournament.Game);
                await tournament.Save();
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
