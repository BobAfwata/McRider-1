using McRider.Common.Services;
using McRider.MAUI.Extensions;
using McRider.MAUI.Services;

namespace McRider.MAUI
{
    public class ServiceRegistry : BaseServiceRegistry
    {
        public override IServiceCollection AddServices(IServiceCollection services)
        {
            // Register Services
            services.AddSingleton<ServiceWorker>();
            services.AddSingletonIfMissing<IForegroundService, ForegroundService>();
            services.AddSingletonIfMissing<IBackgroundService, ForegroundService>();

            // Register Views and ViewModels
            AddViews(services);         // Add Views
            AddViewModels(services);    // Add ViewModel

            return services;
        }
        
        private static IServiceCollection AddViews(IServiceCollection services)
        {
            services.AddView<LoadingPage>();
            services.AddView<SliderPage>();
            services.AddView<LandingPage>();
            services.AddView<GamesPage>();
            services.AddView<RegistrationPage>();
            services.AddView<StartGamePage>();
            services.AddView<MatchupPage>();
            services.AddView<MatchupUnveilPage>();

            return services;
        }

        private static IServiceCollection AddViewModels(IServiceCollection services)
        {
            services.AddViewModel<LoadingPageViewModel>();
            services.AddViewModel<SliderPageViewModel>();
            services.AddViewModel<LandingPageViewModel>();
            services.AddViewModel<GamesPageViewModel>();
            services.AddViewModel<RegistrationPageViewModel>();
            services.AddViewModel<StartGamePageViewModel>();
            services.AddViewModel<MatchupPageViewModel>();

            return services;
        }
    }
}
