[![NuGet](https://img.shields.io/nuget/v/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![Downloads](https://img.shields.io/nuget/dt/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

# AssemblyServiceRegistrar

Marker interface'ler ve attribute'lar aracılığıyla servisleri otomatik kaydeden bir .NET kütüphanesi — tüm lifetime'ları ve open generic'leri destekler.

> 🇬🇧 For English see [README.md](README.md)

---

## Hızlı Başlangıç

```bash
dotnet add package AssemblyServiceRegistrar
```

```csharp
// Interface'inizi bir lifetime marker ile işaretleyin
public interface IUserService : IScopedService { ... }
public class UserService : IUserService { ... }

// Her şeyi tek çağrıda kaydedin
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

Hepsi bu kadar — manuel kayıt gerekmez.

---

## Özellikler

- **Otomatik Assembly Tarama** — bir veya birden fazla assembly'yi tarar ve servisleri otomatik kaydeder
- **Marker Interface Pattern** — `ISingletonService`, `IScopedService` veya `ITransientService` ile lifetime tanımlama
- **Attribute Desteği** — `[ServiceRegistration]` ile alternatif kullanım, self-registration dahil
- **Open Generic Desteği** — `IRepository<T>` gibi açık generic servisleri kaydeder
- **Filtreleme** — `Func<Type, bool>` predicate ile tip dahil etme/dışlama
- **Güvenli Kayıt** — duplicate kaydı önlemek için opsiyonel `tryAdd`
- **Dayanıklı Tarama** — `ReflectionTypeLoadException` durumunda yüklenebilen tipleri kullanır

---

## Gereksinimler

- .NET 8.0 or later
- `Microsoft.Extensions.DependencyInjection`

---

## Kurulum

```bash
dotnet add package AssemblyServiceRegistrar
```

```powershell
# Package Manager Console
Install-Package AssemblyServiceRegistrar
```

```xml
<PackageReference Include="AssemblyServiceRegistrar" Version="1.0.0" />
```

### Kaynak koddan derleme

```bash
git clone https://github.com/sametbrr/AssemblyServiceRegistrar.git
cd AssemblyServiceRegistrar
dotnet build
```

---

## Kullanım

### Marker interface'ler

```csharp
using AssemblyServiceRegistrar;

public interface IConfigurationService : ISingletonService
{
    string GetConnectionString();
}

public interface IUserService : IScopedService
{
    Task<User> GetUserAsync(int id);
    Task CreateUserAsync(User user);
}

public interface IEmailService : ITransientService
{
    Task SendEmailAsync(string to, string subject, string body);
}
```

### Tek assembly'den kayıt

```csharp
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

### Birden fazla assembly'den kayıt

```csharp
builder.Services.AddServicesFromAssemblies(
    Assembly.GetExecutingAssembly(),
    typeof(SomeTypeInAnotherAssembly).Assembly);
```

### Attribute ile kayıt

```csharp
// Self-registration
[ServiceRegistration(ServiceLifetime.Singleton)]
public class CacheManager { }

// Açık servis tipi
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IReportService))]
public class ReportService : IReportService { }
```

### Open generic'ler

```csharp
public interface IRepository<T> : IScopedService { }
public class Repository<T> : IRepository<T> { }

// IRepository<> → Repository<> otomatik kaydedilir
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

### Filtreleme ve güvenli kayıt

```csharp
// Belirli namespace'leri dışla
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    filter: t => !t.Namespace!.Contains("Internal"));

// Duplicate kaydı engelle
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    tryAdd: true);
```

---

## Nasıl Çalışır

1. Belirtilen assembly'lerdeki tüm somut (soyut olmayan) sınıfları tarar.
2. Bir sınıf, `IService` türevi bir interface implement ediyorsa veya `[ServiceRegistration]` taşıyorsa aday kabul edilir.
3. Lifetime belirler:
   - Varsa `[ServiceRegistration(lifetime)]` önceliklidir
   - `ISingletonService` → `ServiceLifetime.Singleton`
   - `IScopedService` → `ServiceLifetime.Scoped`
   - `ITransientService` → `ServiceLifetime.Transient`
   - **Birden fazla** lifetime marker implement eden sınıf `InvalidOperationException` fırlatır
4. Sınıfı, implement ettiği her `IService` türevi interface'e kaydeder (marker interface'ler hariç); open generic'ler açık generic tanımı olarak kaydedilir.

---

## Marker Interface'ler

```csharp
namespace AssemblyServiceRegistrar
{
    public interface IService { }
    public interface IScopedService : IService { }
    public interface ISingletonService : IService { }
    public interface ITransientService : IService { }
}
```

---

## En İyi Uygulamalar

- Stateless servisler ve konfigürasyonlar için `ISingletonService` kullanın
- Request scope içinde paylaşılması gereken servisler için `IScopedService` kullanın
- Hafif, stateless servisler için `ITransientService` kullanın
- Servis interface'lerinizi odaklı tutun ve Single Responsibility Principle'ı takip edin

---

## Sınırlamalar

- Interface tabanlı kayıt yalnızca `IService` türevi interface'leri kapsar — diğerleri veya self-registration için `[ServiceRegistration]` kullanın
- Bir sınıf birden fazla lifetime marker interface implement edemez
- Constructor/parametre tabanlı koşullu kayıt desteklenmez

---

## Lisans

MIT — bkz. [LICENSE.txt](LICENSE.txt).
