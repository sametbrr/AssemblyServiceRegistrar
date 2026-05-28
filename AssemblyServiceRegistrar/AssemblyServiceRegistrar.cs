using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace AssemblyServiceRegistrar
{
    public static class AssemblyServiceRegistrar
    {
        private static readonly HashSet<Type> MarkerInterfaces = new()
        {
            typeof(IService),
            typeof(IScopedService),
            typeof(ISingletonService),
            typeof(ITransientService),
        };

        public static IServiceCollection AddServicesFromAssembly(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
            Func<Type, bool>? filter = null,
            bool tryAdd = false)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            return services.AddServicesFromAssemblies(new[] { assembly }, defaultLifetime, filter, tryAdd);
        }

        public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.AddServicesFromAssemblies(assemblies, ServiceLifetime.Transient);
        }

        public static IServiceCollection AddServicesFromAssemblies(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime defaultLifetime = ServiceLifetime.Transient,
            Func<Type, bool>? filter = null,
            bool tryAdd = false)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));

            var implementations = assemblies
                .Where(a => a is not null)
                .SelectMany(GetLoadableTypes)
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => filter is null || filter(t));

            foreach (var impl in implementations)
            {
                var attribute = impl.GetCustomAttribute<ServiceRegistrationAttribute>(inherit: false);

                var serviceInterfaces = impl.GetInterfaces()
                    .Where(i => typeof(IService).IsAssignableFrom(i) && !MarkerInterfaces.Contains(i))
                    .ToArray();

                if (attribute is null && serviceInterfaces.Length == 0) continue;

                var lifetime = attribute?.Lifetime ?? ResolveLifetime(impl, defaultLifetime);

                foreach (var serviceType in ResolveServiceTypes(impl, attribute, serviceInterfaces))
                {
                    var descriptor = new ServiceDescriptor(serviceType, impl, lifetime);

                    if (tryAdd)
                        services.TryAdd(descriptor);
                    else
                        services.Add(descriptor);
                }
            }

            return services;
        }

        private static IEnumerable<Type> ResolveServiceTypes(Type impl, ServiceRegistrationAttribute? attribute, Type[] serviceInterfaces)
        {
            if (attribute?.ServiceType is not null)
            {
                yield return NormalizeServiceType(attribute.ServiceType, impl);
                yield break;
            }

            if (serviceInterfaces.Length > 0)
            {
                foreach (var iface in serviceInterfaces)
                    yield return NormalizeServiceType(iface, impl);
                yield break;
            }

            // Servis interface'i ve explicit tip yok → kendi tipine kaydet (self-registration)
            yield return impl;
        }

        private static Type NormalizeServiceType(Type serviceType, Type impl)
        {
            if (impl.IsGenericTypeDefinition && serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition)
                return serviceType.GetGenericTypeDefinition();

            return serviceType;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t is not null)!;
            }
        }

        private static ServiceLifetime ResolveLifetime(Type impl, ServiceLifetime defaultLifetime)
        {
            var markers = 0;
            if (typeof(ISingletonService).IsAssignableFrom(impl)) markers++;
            if (typeof(IScopedService).IsAssignableFrom(impl)) markers++;
            if (typeof(ITransientService).IsAssignableFrom(impl)) markers++;

            if (markers > 1)
                throw new InvalidOperationException(
                    $"'{impl.FullName}' birden fazla lifetime marker interface implement ediyor. " +
                    "Bir tip yalnızca ISingletonService, IScopedService veya ITransientService'ten birini implement etmelidir.");

            if (typeof(ISingletonService).IsAssignableFrom(impl)) return ServiceLifetime.Singleton;
            if (typeof(IScopedService).IsAssignableFrom(impl)) return ServiceLifetime.Scoped;
            if (typeof(ITransientService).IsAssignableFrom(impl)) return ServiceLifetime.Transient;
            return defaultLifetime;
        }
    }
}
