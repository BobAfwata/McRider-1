using McRider.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace McRider.Common.Extensions;
public static class IocExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        // Get all assemblies in the current application domain
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Running Assemply
        var assmbly = Assembly.GetExecutingAssembly();

        // Find all types that inherit from ServiceManager in all assemblies
        var serviceManagerTypes = assemblies
            .OrderBy(ns => ns == assmbly ? 1 : 0)
            .SelectMany(ns => ns.DefinedTypes ?? new List<TypeInfo>())
            .Where(type => type?.IsAbstract == false && type.IsAssignableTo(typeof(BaseServiceRegistry)))
            .ToList();

        // Create an instance of each ServiceManager and call AddServices
        foreach (var serviceManagerType in serviceManagerTypes)
        {
            var serviceManagerInstance = Activator.CreateInstance(serviceManagerType);
            var addServicesMethod = serviceManagerType.GetMethod("AddServices", new[] { typeof(IServiceCollection) });

            if (addServicesMethod == null) continue;

            try
            {
                addServicesMethod.Invoke(serviceManagerInstance, new[] { serviceCollection });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        return serviceCollection;
    }

    public static bool IsServiceAdded<T>(this IServiceCollection service)
    {
        var serviceDescriptor = service.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
        return serviceDescriptor != null;
    }

    public static IServiceCollection AddSingletonIfMissing<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
    {
        if (!services.IsServiceAdded<TService>())
            services.AddSingleton<TService, TImplementation>();

        return services;
    }
}

public abstract class BaseServiceRegistry
{
    public abstract IServiceCollection AddServices(IServiceCollection services);
}


public class CommonServiceRegistry : BaseServiceRegistry
{
    public override IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddScoped<FileCacheService>();
        services.AddScoped<RetryExecutionService>();
        services.AddScoped(typeof(RepositoryService<>), typeof(RepositoryService<>));

        return services;
    }
}