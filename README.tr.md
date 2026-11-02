[![NuGet](https://img.shields.io/nuget/v/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![Downloads](https://img.shields.io/nuget/dt/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

[Hızlı Başlangıç](#hızlı-başlangıç) • [Kurulum](#kurulum) • [Kullanım](#kullanım) • [Nasıl Çalışır](#nasıl-çalışır) • [Sınırlamalar](#sınırlamalar) • [Katkıda Bulunma](#katkıda-bulunma)

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

| | |
|---|---|
| **Otomatik assembly tarama** | Bir veya birden fazla assembly'yi tarar ve servisleri otomatik kaydeder |
| **Marker interface pattern** | Lifetime'ı `ISingletonService`, `IScopedService` veya `ITransientService` belirler |
| **Attribute desteği** | Alternatif olarak `[ServiceRegistration]`, self-registration dahil |
| **Open generic desteği** | `IRepository<T>` gibi açık generic servisleri kaydeder |
| **Filtreleme** | `Func<Type, bool>` predicate ile tip dahil etme veya dışlama |
| **Güvenli kayıt** | Opsiyonel `tryAdd` duplicate kayıtları önler |
| **Dayanıklı tarama** | `ReflectionTypeLoadException`'da yüklenemeyen tipleri atlayarak devam eder |

---

## Gereksinimler

- .NET 8.0 veya üstü
- `Microsoft.Extensions.DependencyInjection`

---

## Kurulum

```bash
dotnet add package AssemblyServiceRegistrar
```

```powershell
# Package Manager Console içinde
Install-Package AssemblyServiceRegistrar
```

```xml
<PackageReference Include="AssemblyServiceRegistrar" Version="1.1.1" />
```

### Kaynak koddan derleme

```bash
git clone https://github.com/sametbrr/AssemblyServiceRegistrar.git
cd AssemblyServiceRegistrar
dotnet build
```

---

## Kullanım

### Interface'lerde lifetime belirtme

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
// Kendi tipine kayıt (self-registration)
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

## Sınırlamalar

**Yalnızca `IService` türevi interface'ler servis tipi olur.** Tarama, bir sınıfı implement ettiği interface'lere yalnızca bunlar `IService`'ten türüyorsa kaydeder; böylece `IDisposable` gibi ilgisiz interface'ler yanlışlıkla kaydedilmez.

**Geçici çözüm:** sınıfa `[ServiceRegistration(lifetime, typeof(IYourInterface))]` ekleyin ya da sınıfı kendi tipine kaydetmek için servis tipini boş bırakın.

**Sınıf başına tek lifetime marker.** `ISingletonService`, `IScopedService` ve `ITransientService`'ten birden fazlasını implement eden bir sınıfın tek bir lifetime'ı olmaz; tarama `InvalidOperationException` fırlatır.

**Geçici çözüm:** her interface hiyerarşisinde tek marker kullanın ya da lifetime'ı sınıf üzerinde `[ServiceRegistration]` ile belirtin; attribute marker'lardan önceliklidir.

**Koşullu kayıt yok.** Karar başlangıçta yalnızca tiplere bakılarak verilir; constructor parametreleri, konfigürasyon ve ortam değerlendirilmez.

**Geçici çözüm:** tipi `filter` predicate'i ile dışlayın ve ihtiyacınız olan koşullarla manuel kaydedin.

---

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

## Katkıda Bulunma

- Sorular ve fikirler: [Discussions](https://github.com/sametbrr/AssemblyServiceRegistrar/discussions)
- Hatalar: [issue takibi](https://github.com/sametbrr/AssemblyServiceRegistrar/issues)
- Pull request'ler: önce [katkı rehberini](https://github.com/sametbrr/.github/blob/main/CONTRIBUTING.md) okuyun
- Güvenlik bildirimleri: herkese açık issue yerine [güvenlik politikasını](https://github.com/sametbrr/AssemblyServiceRegistrar/security/policy) izleyin

---

## Lisans

MIT — bkz. [LICENSE.txt](LICENSE.txt).

---

<div align="center">

[Hata Bildir](https://github.com/sametbrr/AssemblyServiceRegistrar/issues) · [Özellik İste](https://github.com/sametbrr/AssemblyServiceRegistrar/issues)

</div>
