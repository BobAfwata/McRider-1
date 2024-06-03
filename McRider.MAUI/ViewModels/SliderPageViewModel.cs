using McRider.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.ViewModels
{
    public partial class SliderPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<SliderItem> _items = [];
        private FileCacheService _fileCacheService;

        public SliderPageViewModel(FileCacheService fileCacheService)
        {
            _fileCacheService = fileCacheService;
        }

        override public async Task Initialize(params object[] args)
        {
            if (App.Configs?.Theme == "showmax")
            {
                await LetsPlay();
                return;
            }

            Items = new ObservableCollection<SliderItem>(await _fileCacheService.GetAsync(App.Configs?.Theme + ".slider-items.json", GetItemsAsync));
            await base.Initialize(args);
        }

        [RelayCommand]
        private async Task LetsPlay()
        {
            await Shell.Current.GoToAsync($"///{nameof(LandingPage)}"); ;
        }

        private async Task<SliderItem[]> GetItemsAsync()
        {
            

            return [
                new SliderItem { Image = "founding_year_inverted.png", Title = "Image 1", Description = "Description 1" },
                new SliderItem { Image = "founding_year_transparent.png", Title = "Image 2", Description = "Description 2" },
                new SliderItem { Image = "founding_year_yellow.png", Title = "Image 3", Description = "Description 3" },
            ];
        }

        public class SliderItem
        {
            public string Image { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }   
    }
}
