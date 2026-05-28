# Assembly Service Registrar

**EN:** Assembly Service Registrar is a .NET library that provides automatic service registration for dependency injection containers using marker interfaces. This library simplifies the process of registering services by automatically scanning assemblies and registering implementations based on interface markers.

**TR:** Assembly Service Registrar, marker interface'ler kullanarak dependency injection container'ları için otomatik servis kaydı sağlayan bir .NET kütüphanesidir. Bu kütüphane, assembly'leri otomatik olarak tarayarak ve interface marker'larına göre implementation'ları kaydederek servis kayıt işlemini basitleştirir.

## 🚀 Features | Özellikler

**EN:**
- **Automatic Assembly Scanning**: Scans one or more assemblies and automatically registers services
- **Marker Interface Pattern**: Uses marker interfaces to determine service lifetimes
- **Attribute Support**: `[ServiceRegistration]` as an alternative to marker interfaces (with self-registration)
- **Open Generic Support**: Registers open generic services such as `IRepository<T>`
- **Filtering**: Include/exclude types via a `Func<Type, bool>` predicate
- **Safe Registration**: Optional `tryAdd` to avoid duplicate registrations
- **Lifetime Management**: Supports Singleton, Scoped, and Transient lifetimes
- **Resilient Scanning**: Tolerates `ReflectionTypeLoadException` (skips unloadable types)
- **Simple Integration**: Easy integration with Microsoft.Extensions.DependencyInjection

**TR:**
- **Otomatik Assembly Tarama**: Bir veya birden fazla assembly'yi tarar ve servisleri otomatik kaydeder
- **Marker Interface Pattern**: Servis lifetime'larını belirlemek için marker interface'ler kullanır
- **Attribute Desteği**: Marker'a alternatif `[ServiceRegistration]` (self-registration dahil)
- **Open Generic Desteği**: `IRepository<T>` gibi açık generic servisleri kaydeder
- **Filtreleme**: `Func<Type, bool>` predicate ile tip dahil etme/dışlama
- **Güvenli Kayıt**: Duplicate kaydı önlemek için opsiyonel `tryAdd`
- **Lifetime Yönetimi**: Singleton, Scoped ve Transient lifetime'larını destekler
- **Dayanıklı Tarama**: `ReflectionTypeLoadException` durumunda yüklenebilen tipleri kullanır
- **Basit Entegrasyon**: Microsoft.Extensions.DependencyInjection ile kolay entegrasyon

## 📋 Requirements | Gereksinimler

- .NET 8.0 / .NET 9.0 / .NET 10.0
- Microsoft.Extensions.DependencyInjection package

## 🔧 Installation | Kurulum

### Package Installation | Paket Kurulumu

**EN:** Add the package to your project using one of the following methods:

**TR:** Aşağıdaki yöntemlerden birini kullanarak paketi projenize ekleyin:

#### NuGet Package Manager

```bash
dotnet add package AssemblyServiceRegistrar
```

#### Package Manager Console (Visual Studio)

```powershell
Install-Package AssemblyServiceRegistrar
```

#### PackageReference (in .csproj file)

```xml
<PackageReference Include="AssemblyServiceRegistrar" Version="1.0.0" />
```

### Git Clone

```bash
git clone https://github.com/sametbrr/AssemblyServiceRegistrar.git
cd AssemblyServiceRegistrar
dotnet build
```

## 🚀 Usage | Kullanım

### Basic Usage | Temel Kullanım

**EN:** First, create your service interfaces by inheriting from the appropriate marker interfaces:

**TR:** Öncelikle, uygun marker interface'lerden türeterek servis interface'lerinizi oluşturun:

```csharp
using AssemblyServiceRegistrar;

// For Singleton services | Singleton servisler için
public interface IConfigurationService : ISingletonService
{
    string GetConnectionString();
}

// For Scoped services | Scoped servisler için  
public interface IUserService : IScopedService
{
    Task<User> GetUserAsync(int id);
    Task CreateUserAsync(User user);
}

// For Transient services | Transient servisler için
public interface IEmailService : ITransientService
{
    Task SendEmailAsync(string to, string subject, string body);
}
```

### Service Implementations | Servis Implementation'ları

```csharp
// Singleton service implementation
public class ConfigurationService : IConfigurationService
{
    public string GetConnectionString()
    {
        return "Server=localhost;Database=MyApp;";
    }
}

// Scoped service implementation
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    
    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User> GetUserAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
    
    public async Task CreateUserAsync(User user)
    {
        await _userRepository.AddAsync(user);
    }
}

// Transient service implementation
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Email sending logic
        await Task.CompletedTask;
    }
}
```

### Complete Example | Tam Örnek

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AssemblyServiceRegistrar;
using System.Reflection;

// Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());

var host = builder.Build();
```

**EN:** To scan multiple assemblies at once:

**TR:** Birden fazla assembly'yi tek seferde taramak için:

```csharp
builder.Services.AddServicesFromAssemblies(
    Assembly.GetExecutingAssembly(),
    typeof(SomeTypeInAnotherAssembly).Assembly);
```

### Attribute-based Registration | Attribute ile Kayıt

**EN:** As an alternative to marker interfaces, decorate a class with `[ServiceRegistration]`. The attribute overrides marker-based lifetime resolution. If no service type is given, the class is registered against its `IService`-derived interfaces, or against itself (self-registration) when it has none.

**TR:** Marker interface'lere alternatif olarak bir sınıfı `[ServiceRegistration]` ile işaretleyebilirsiniz. Attribute, marker tabanlı lifetime çözümlemesini geçersiz kılar. Servis tipi verilmezse sınıf, `IService` türevi interface'lerine; hiç yoksa kendi tipine (self-registration) kaydedilir.

```csharp
using AssemblyServiceRegistrar;
using Microsoft.Extensions.DependencyInjection;

// Self-registration | Kendi tipine kayıt
[ServiceRegistration(ServiceLifetime.Singleton)]
public class CacheManager { }

// Explicit service type | Açık servis tipi
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IReportService))]
public class ReportService : IReportService { }
```

### Open Generic Services | Open Generic Servisler

```csharp
public interface IRepository<T> : IScopedService { }
public class Repository<T> : IRepository<T> { }

// IRepository<> -> Repository<> olarak kaydedilir
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());

// var repo = provider.GetRequiredService<IRepository<User>>(); // Repository<User>
```

### Filtering & Safe Registration | Filtreleme ve Güvenli Kayıt

```csharp
// Belirli tipleri dışla | Exclude specific types
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    filter: t => !t.Namespace!.Contains("Internal"));

// Duplicate kaydı engelle | Avoid duplicate registrations
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    tryAdd: true);
```

## 🔍 How It Works | Nasıl Çalışır

**EN:**
1. The library scans all concrete (non-abstract) classes in the specified assemblies
2. A class is a candidate if it implements an `IService`-derived interface or is decorated with `[ServiceRegistration]`
3. Determines the service lifetime:
   - `[ServiceRegistration(lifetime)]` takes precedence if present
   - `ISingletonService` → `ServiceLifetime.Singleton`
   - `IScopedService` → `ServiceLifetime.Scoped`
   - `ITransientService` → `ServiceLifetime.Transient`
   - Otherwise uses the provided default lifetime
   - A class implementing **more than one** lifetime marker throws `InvalidOperationException`
4. Registers the class against each `IService`-derived interface it implements (marker interfaces excluded); open generics are registered as open generic definitions

**TR:**
1. Kütüphane belirtilen assembly'lerdeki tüm somut (soyut olmayan) sınıfları tarar
2. Bir sınıf, `IService` türevi bir interface implement ediyorsa veya `[ServiceRegistration]` ile işaretliyse aday kabul edilir
3. Servis lifetime'ını belirler:
   - Varsa `[ServiceRegistration(lifetime)]` önceliklidir
   - `ISingletonService` → `ServiceLifetime.Singleton`
   - `IScopedService` → `ServiceLifetime.Scoped`
   - `ITransientService` → `ServiceLifetime.Transient`
   - Aksi halde sağlanan varsayılan lifetime kullanılır
   - **Birden fazla** lifetime marker implement eden sınıf `InvalidOperationException` fırlatır
4. Sınıfı, implement ettiği her `IService` türevi interface'e kaydeder (marker interface'ler hariç); open generic'ler açık generic tanımı olarak kaydedilir

## 📚 Marker Interfaces | Marker Interface'ler

```csharp
namespace AssemblyServiceRegistrar
{
    // Base marker interface | Temel marker interface
    public interface IService { }
    
    // Scoped lifetime marker | Scoped lifetime marker'ı
    public interface IScopedService : IService { }
    
    // Singleton lifetime marker | Singleton lifetime marker'ı  
    public interface ISingletonService : IService { }
    
    // Transient lifetime marker | Transient lifetime marker'ı
    public interface ITransientService : IService { }
}
```
## 🎯 Best Practices | En İyi Uygulamalar

**EN:**
- Use `ISingletonService` for stateless services and configurations
- Use `IScopedService` for services that should be shared within a request scope
- Use `ITransientService` for lightweight, stateless services
- Keep your service interfaces focused and follow the Single Responsibility Principle

**TR:**
- Stateless servisler ve konfigürasyonlar için `ISingletonService` kullanın
- Request scope içinde paylaşılması gereken servisler için `IScopedService` kullanın
- Hafif, stateless servisler için `ITransientService` kullanın
- Servis interface'lerinizi odaklı tutun ve Single Responsibility Principle'ı takip edin

## ⚠️ Limitations | Sınırlamalar

**EN:**
- Interface-based registration only covers interfaces deriving from `IService` (use `[ServiceRegistration]` for others or self-registration)
- A class must not implement more than one lifetime marker interface (throws)
- Constructor/parameter-based conditional registration is not supported

**TR:**
- Interface tabanlı kayıt yalnızca `IService` türevi interface'leri kapsar (diğerleri veya self-registration için `[ServiceRegistration]` kullanın)
- Bir sınıf birden fazla lifetime marker interface implement edemez (exception fırlatır)
- Constructor/parametre tabanlı koşullu kayıt desteklenmez

## 📄 License | Lisans

This project is licensed under the MIT License

Bu proje MIT Lisansı altında lisanslanmıştır
