using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.ViewModels
{
    public partial class LandingPageViewModel : BaseViewModel
    {
        [RelayCommand]
        async Task TakeChallenge()
        {
            await Shell.Current.GoToAsync($"{nameof(GamesPage)}");
        }
    }
}
