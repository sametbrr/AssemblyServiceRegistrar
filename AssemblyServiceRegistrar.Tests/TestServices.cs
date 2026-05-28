using AssemblyServiceRegistrar;

namespace AssemblyServiceRegistrar.Tests
{
    // Hedefli testlerde tam-assembly taramasının dışında tutulacak fixture'lar için tag.
    // IService türevi DEĞİL — yani asla register edilmez, sadece filtre amaçlı.
    public interface IIsolatedFixture { }

    public interface ISingletonSample : ISingletonService { }
    public class SingletonSample : ISingletonSample { }

    public interface IScopedSample : IScopedService { }
    public class ScopedSample : IScopedSample { }

    public interface ITransientSample : ITransientService { }
    public class TransientSample : ITransientSample { }

    // Marker'sız: defaultLifetime kullanılmalı
    public interface IDefaultSample : IService { }
    public class DefaultSample : IDefaultSample { }

    // Soyut sınıf register edilmemeli
    public interface IAbstractSample : ITransientService { }
    public abstract class AbstractSample : IAbstractSample { }

    // Open generic servis — hedefli filtre ile test edilir
    public interface IRepository<T> : IScopedService { }
    public class Repository<T> : IRepository<T>, IIsolatedFixture { }

    // Çoklu lifetime marker çakışması — hedefli filtre ile test edilir
    public interface IConflictSample : IService { }
    public class MultiMarkerConflict : IConflictSample, ISingletonService, ITransientService, IIsolatedFixture { }

    // Attribute ile self-registration (servis interface'i yok)
    [ServiceRegistration(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class AttributeSelfSample : IIsolatedFixture { }

    // Attribute ile explicit servis tipi
    public interface IAttributeSample { }
    [ServiceRegistration(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, typeof(IAttributeSample))]
    public class AttributeSample : IAttributeSample, IIsolatedFixture { }
}
