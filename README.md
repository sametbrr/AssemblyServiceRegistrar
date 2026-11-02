[![NuGet](https://img.shields.io/nuget/v/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![Downloads](https://img.shields.io/nuget/dt/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

[Quick Start](#quick-start) • [Installation](#installation) • [Usage](#usage) • [How It Works](#how-it-works) • [Limitations](#limitations) • [Contributing](#contributing)

# AssemblyServiceRegistrar

A .NET library that auto-registers services via marker interfaces and attributes — supports all lifetimes and open generics.

> 🇹🇷 Türkçe için [README.tr.md](README.tr.md)

---

## Quick Start

```bash
dotnet add package AssemblyServiceRegistrar
```

```csharp
// Mark your interfaces with a lifetime marker
public interface IUserService : IScopedService { ... }
public class UserService : IUserService { ... }

// Register everything in one call
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

That's it — no manual registration needed.

---

## Features

| | |
|---|---|
| **Automatic assembly scanning** | Scans one or more assemblies and registers services automatically |
| **Marker interface pattern** | `ISingletonService`, `IScopedService` or `ITransientService` declares the lifetime |
| **Attribute support** | `[ServiceRegistration]` as an alternative, including self-registration |
| **Open generic support** | Registers open generic services such as `IRepository<T>` |
| **Filtering** | Include or exclude types with a `Func<Type, bool>` predicate |
| **Safe registration** | Optional `tryAdd` avoids duplicate registrations |
| **Resilient scanning** | Tolerates `ReflectionTypeLoadException` by skipping unloadable types |

---

## Requirements

- .NET 8.0 or later
- `Microsoft.Extensions.DependencyInjection`

---

## Installation

```bash
dotnet add package AssemblyServiceRegistrar
```

```powershell
# Package Manager Console
Install-Package AssemblyServiceRegistrar
```

```xml
<PackageReference Include="AssemblyServiceRegistrar" Version="1.1.1" />
```

### Build from source

```bash
git clone https://github.com/sametbrr/AssemblyServiceRegistrar.git
cd AssemblyServiceRegistrar
dotnet build
```

---

## Usage

### Declare lifetimes on your interfaces

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

### Register from one assembly

```csharp
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

### Register from multiple assemblies

```csharp
builder.Services.AddServicesFromAssemblies(
    Assembly.GetExecutingAssembly(),
    typeof(SomeTypeInAnotherAssembly).Assembly);
```

### Attribute-based registration

```csharp
// Self-registration
[ServiceRegistration(ServiceLifetime.Singleton)]
public class CacheManager { }

// Explicit service type
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IReportService))]
public class ReportService : IReportService { }
```

### Open generics

```csharp
public interface IRepository<T> : IScopedService { }
public class Repository<T> : IRepository<T> { }

// IRepository<> → Repository<> registered automatically
builder.Services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
```

### Filtering and safe registration

```csharp
// Exclude specific namespaces
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    filter: t => !t.Namespace!.Contains("Internal"));

// Avoid duplicate registrations
builder.Services.AddServicesFromAssembly(
    Assembly.GetExecutingAssembly(),
    tryAdd: true);
```

---

## How It Works

1. Scans all concrete (non-abstract) classes in the specified assemblies.
2. A class is a candidate if it implements an `IService`-derived interface or carries `[ServiceRegistration]`.
3. Determines lifetime:
   - `[ServiceRegistration(lifetime)]` takes precedence if present
   - `ISingletonService` → `ServiceLifetime.Singleton`
   - `IScopedService` → `ServiceLifetime.Scoped`
   - `ITransientService` → `ServiceLifetime.Transient`
   - A class implementing **more than one** lifetime marker throws `InvalidOperationException`
4. Registers against each `IService`-derived interface (marker interfaces excluded); open generics are registered as open generic definitions.

---

## Limitations

**Only `IService`-derived interfaces become service types.** Scanning registers a class against the interfaces it implements only when they derive from `IService`, so unrelated interfaces such as `IDisposable` are never registered by accident.

**Workaround:** add `[ServiceRegistration(lifetime, typeof(IYourInterface))]` to the class, or leave out the service type to register the class as itself.

**One lifetime marker per class.** A class that implements more than one of `ISingletonService`, `IScopedService` and `ITransientService` has no single lifetime, so scanning throws `InvalidOperationException`.

**Workaround:** keep one marker per interface hierarchy, or set the lifetime on the class with `[ServiceRegistration]`, which takes precedence over markers.

**No conditional registration.** The decision is made from types alone at startup; constructor parameters, configuration and environment are not evaluated.

**Workaround:** exclude the type with the `filter` predicate and register it manually under the conditions you need.

---

---

## Marker Interfaces

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

## Best Practices

- Use `ISingletonService` for stateless services and configurations
- Use `IScopedService` for services shared within a request scope
- Use `ITransientService` for lightweight, stateless services
- Keep interfaces focused and follow the Single Responsibility Principle

---

## Contributing

- Questions and ideas: [Discussions](https://github.com/sametbrr/AssemblyServiceRegistrar/discussions)
- Bugs: [issue tracker](https://github.com/sametbrr/AssemblyServiceRegistrar/issues)
- Pull requests: read the [contribution guide](https://github.com/sametbrr/.github/blob/main/CONTRIBUTING.md) first
- Security reports: follow the [security policy](https://github.com/sametbrr/AssemblyServiceRegistrar/security/policy), not a public issue

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).

---

<div align="center">

[Report Bug](https://github.com/sametbrr/AssemblyServiceRegistrar/issues) · [Request Feature](https://github.com/sametbrr/AssemblyServiceRegistrar/issues)

</div>
