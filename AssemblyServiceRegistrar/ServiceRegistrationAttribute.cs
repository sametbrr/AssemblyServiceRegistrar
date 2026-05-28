using Microsoft.Extensions.DependencyInjection;

namespace AssemblyServiceRegistrar
{
    /// <summary>
    /// TR - Marker interface'e alternatif olarak bir sınıfın lifetime'ını ve
    /// (opsiyonel) hangi servis tipine kaydedileceğini açıkça belirtir.
    /// EN - Explicitly declares a class's lifetime and, optionally, the service type
    /// it should be registered as — an alternative to marker interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ServiceRegistrationAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// TR - Kaydedilecek servis tipi. null ise; sınıfın IService türevi
        /// interface'leri, yoksa sınıfın kendisi kullanılır.
        /// EN - The service type to register against. When null, the class's
        /// IService-derived interfaces are used, or the class itself if it has none.
        /// </summary>
        public Type? ServiceType { get; }

        public ServiceRegistrationAttribute(ServiceLifetime lifetime, Type? serviceType = null)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }
    }
}
