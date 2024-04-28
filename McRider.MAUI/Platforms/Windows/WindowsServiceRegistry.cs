using McRider.Common.Extensions;
using McRider.MAUI.Platforms.Windows;
using McRider.MAUI.Platforms.Windows.Services;
using McRider.MAUI.Services;

namespace McRider.MAUI.Platforms.Android
{
    public class WindowsServiceManager : BaseServiceRegistry
    {
        public override IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddSingleton<ITextMeter, WindowsTextMeter>();
            services.AddSingleton<IScreenSelector, WindowsScreenSelector>();
            services.AddSingleton<ArdrinoCommunicator, WindowsArdrinoSerialPortCommunicator>();

            return services;
        }
    }
}
