[![NuGet](https://img.shields.io/nuget/v/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![Downloads](https://img.shields.io/nuget/dt/AssemblyServiceRegistrar.svg)](https://www.nuget.org/packages/AssemblyServiceRegistrar)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

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

- **Automatic Assembly Scanning** — scans one or more assemblies and registers services automatically
- **Marker Interface Pattern** — use `ISingletonService`, `IScopedService`, or `ITransientService` to declare lifetime
- **Attribute Support** — `[ServiceRegistration]` as an alternative, with self-registration
- **Open Generic Support** — registers open generic services such as `IRepository<T>`
- **Filtering** — include/exclude types via a `Func<Type, bool>` predicate
- **Safe Registration** — optional `tryAdd` to avoid duplicate registrations
- **Resilient Scanning** — tolerates `ReflectionTypeLoadException` (skips unloadable types)

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
<PackageReference Include="AssemblyServiceRegistrar" Version="1.0.0" />
```

### Build from source

```bash
git clone https://github.com/sametbrr/AssemblyServiceRegistrar.git
cd AssemblyServiceRegistrar
dotnet build
```

---

## Usage

### Marker interfaces

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

## Limitations

- Interface-based registration only covers interfaces deriving from `IService` — use `[ServiceRegistration]` for others or self-registration
- A class must not implement more than one lifetime marker interface
- Constructor/parameter-based conditional registration is not supported

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).
