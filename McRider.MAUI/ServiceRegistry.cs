using McRider.MAUI.Extensions;

namespace McRider.MAUI
{
    public class ServiceRegistry : BaseServiceRegistry
    {
        public override IServiceCollection AddServices(IServiceCollection services)
        {
            AddViews(services);         // Add Views
            AddViewModels(services);    // Add ViewModel

            return services;
        }

        private static IServiceCollection AddViews(IServiceCollection services)
        {
            services.AddView<LoadingPage>();
            services.AddView<LandingPage>();
            services.AddView<SliderPage>();

            return services;
        }

        private static IServiceCollection AddViewModels(IServiceCollection services)
        {
            services.AddViewModel<LoadingPageViewModel>();
            services.AddViewModel<LandingPageViewModel>();
            services.AddViewModel<SliderPageViewModel>();

            return services;
        }
    }
}
