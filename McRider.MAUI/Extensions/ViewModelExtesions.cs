using CommunityToolkit.Maui.Views;

namespace McRider.MAUI.Extensions;

public static class ViewViewModelExtensions
{
    public async static Task<object> ShowPopupAsync<V>(this V view)
        where V : View
    {
        var vm = view.BindingContext as BaseDialogViewModel;
        var popup = new DialogWrapper(view, vm);

        return await App.Current.MainPage.ShowPopupAsync(popup);
    }

    public static IServiceCollection AddView<V>(this IServiceCollection services, string route = null, bool singleton = true)
        where V : class
    {
        if (singleton)
            services.AddSingleton<V>();
        else
            services.AddTransient<V>();

        route = string.IsNullOrEmpty(route) ? typeof(V).Name : route;
        try
        {
            var existingRoute = Routing.GetOrCreateContent(route);

            if (existingRoute == null)
                Routing.RegisterRoute(route, typeof(V));
        }
        catch(MissingMethodException)
        {
            //Ignore this error
        }

        return services;
    }

    public static IServiceCollection AddViewModel<VM>(this IServiceCollection services, bool singleton = true)
        where VM : class, INotifyPropertyChanged, INotifyPropertyChanging
    {
        if (singleton)
            services.AddSingleton<VM>();
        else
            services.AddTransient<VM>();

        return services;
    }
}
