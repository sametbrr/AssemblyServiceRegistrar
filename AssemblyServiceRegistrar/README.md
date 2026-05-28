# Assembly Service Registrar

**EN:** Convention-based dependency injection registration for .NET. Scans assemblies and auto-registers services using marker interfaces or the `[ServiceRegistration]` attribute.

**TR:** .NET için convention tabanlı dependency injection kaydı. Assembly'leri tarar ve servisleri marker interface'ler veya `[ServiceRegistration]` attribute'u ile otomatik kaydeder.

## Install

```bash
dotnet add package AssemblyServiceRegistrar
```

## Quick Start

```csharp
using AssemblyServiceRegistrar;
using System.Reflection;

// Servis interface'leri marker'dan türer | Service interfaces derive from a marker
public interface IUserService : IScopedService { }
public class UserService : IUserService { }

// Tek satırda kaydet | Register in one line
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

`ISingletonService` → Singleton, `IScopedService` → Scoped, `ITransientService` → Transient.

## Features

- Single or multiple assembly scanning (`AddServicesFromAssemblies`)
- `[ServiceRegistration(lifetime, serviceType?)]` attribute + self-registration
- Open generic services (`IRepository<T>`)
- `filter` predicate and `tryAdd` (duplicate-safe) options
- Resilient to `ReflectionTypeLoadException`
- Targets net8.0 / net9.0 / net10.0

## More

Full documentation, examples and limitations:
https://github.com/sametbrr/AssemblyServiceRegistrar

MIT License © Samet Birer
