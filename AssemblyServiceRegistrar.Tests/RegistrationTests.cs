using System.Reflection;
using AssemblyServiceRegistrar;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AssemblyServiceRegistrar.Tests
{
    public class RegistrationTests
    {
        private static readonly Assembly TestAssembly = typeof(RegistrationTests).Assembly;

        // Tam-assembly taraması: hedefli testlere ait izole fixture'ları dışla.
        private static bool NotIsolated(Type t) => !typeof(IIsolatedFixture).IsAssignableFrom(t);

        private static IServiceCollection Scan(ServiceLifetime defaultLifetime = ServiceLifetime.Transient)
            => new ServiceCollection().AddServicesFromAssembly(TestAssembly, defaultLifetime, filter: NotIsolated);

        private static ServiceDescriptor? Find<TService>(IServiceCollection services)
            => services.FirstOrDefault(d => d.ServiceType == typeof(TService));

        // ---- Faz 1 ----

        [Fact]
        public void Registers_Singleton_Marker_As_Singleton()
        {
            var descriptor = Find<ISingletonSample>(Scan());

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
            Assert.Equal(typeof(SingletonSample), descriptor.ImplementationType);
        }

        [Fact]
        public void Registers_Scoped_Marker_As_Scoped()
        {
            var descriptor = Find<IScopedSample>(Scan());

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        }

        [Fact]
        public void Registers_Transient_Marker_As_Transient()
        {
            var descriptor = Find<ITransientSample>(Scan());

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Transient, descriptor!.Lifetime);
        }

        [Fact]
        public void Uses_DefaultLifetime_When_No_Marker()
        {
            var descriptor = Find<IDefaultSample>(Scan(ServiceLifetime.Singleton));

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
        }

        [Fact]
        public void Does_Not_Register_Bare_IService_Interface()
        {
            Assert.DoesNotContain(Scan(), d => d.ServiceType == typeof(IService));
        }

        [Fact]
        public void Does_Not_Register_Lifetime_Marker_Interfaces()
        {
            var services = Scan();
            Assert.DoesNotContain(services, d => d.ServiceType == typeof(ISingletonService));
            Assert.DoesNotContain(services, d => d.ServiceType == typeof(IScopedService));
            Assert.DoesNotContain(services, d => d.ServiceType == typeof(ITransientService));
        }

        [Fact]
        public void Does_Not_Register_Abstract_Classes()
        {
            Assert.DoesNotContain(Scan(), d => d.ImplementationType == typeof(AbstractSample));
        }

        [Fact]
        public void Resolves_Registered_Service_From_Provider()
        {
            var provider = Scan().BuildServiceProvider();
            Assert.IsType<SingletonSample>(provider.GetRequiredService<ISingletonSample>());
        }

        [Fact]
        public void AddServicesFromAssemblies_Accepts_Multiple_Assemblies()
        {
            var services = new ServiceCollection()
                .AddServicesFromAssemblies(new[] { TestAssembly, TestAssembly }, filter: NotIsolated);

            // Aynı assembly iki kez → her servis iki kez kaydedilir
            Assert.Equal(2, services.Count(d => d.ServiceType == typeof(ISingletonSample)));
        }

        [Fact]
        public void Throws_When_Services_Is_Null()
        {
            IServiceCollection services = null!;
            Assert.Throws<ArgumentNullException>(() => services.AddServicesFromAssembly(TestAssembly));
        }

        [Fact]
        public void Throws_When_Assembly_Is_Null()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddServicesFromAssembly(null!));
        }

        // ---- Faz 2 ----

        [Fact]
        public void Registers_Open_Generic_Service()
        {
            var services = new ServiceCollection()
                .AddServicesFromAssembly(TestAssembly, filter: t => t == typeof(Repository<>));

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRepository<>));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(Repository<>), descriptor!.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void Open_Generic_Service_Resolves_Closed()
        {
            var provider = new ServiceCollection()
                .AddServicesFromAssembly(TestAssembly, filter: t => t == typeof(Repository<>))
                .BuildServiceProvider();

            Assert.IsType<Repository<int>>(provider.GetRequiredService<IRepository<int>>());
        }

        [Fact]
        public void Throws_On_Multiple_Lifetime_Markers()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(() =>
                services.AddServicesFromAssembly(TestAssembly, filter: t => t == typeof(MultiMarkerConflict)));
        }

        [Fact]
        public void Filter_Excludes_Matching_Types()
        {
            var services = new ServiceCollection()
                .AddServicesFromAssembly(TestAssembly, filter: t => NotIsolated(t) && t != typeof(SingletonSample));

            Assert.DoesNotContain(services, d => d.ImplementationType == typeof(SingletonSample));
            Assert.Contains(services, d => d.ImplementationType == typeof(ScopedSample));
        }

        [Fact]
        public void TryAdd_Prevents_Duplicate_Registration()
        {
            var services = new ServiceCollection();
            services.AddServicesFromAssembly(TestAssembly, filter: NotIsolated, tryAdd: true);
            services.AddServicesFromAssembly(TestAssembly, filter: NotIsolated, tryAdd: true);

            Assert.Equal(1, services.Count(d => d.ServiceType == typeof(ISingletonSample)));
        }

        // ---- Faz 3: Attribute ----

        [Fact]
        public void Attribute_Self_Registers_With_Lifetime()
        {
            var services = new ServiceCollection()
                .AddServicesFromAssembly(TestAssembly, filter: t => t == typeof(AttributeSelfSample));

            var descriptor = Find<AttributeSelfSample>(services);

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
            Assert.Equal(typeof(AttributeSelfSample), descriptor.ImplementationType);
        }

        [Fact]
        public void Attribute_Registers_With_Explicit_ServiceType()
        {
            var services = new ServiceCollection()
                .AddServicesFromAssembly(TestAssembly, filter: t => t == typeof(AttributeSample));

            var descriptor = Find<IAttributeSample>(services);

            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
            Assert.Equal(typeof(AttributeSample), descriptor.ImplementationType);
        }
    }
}
