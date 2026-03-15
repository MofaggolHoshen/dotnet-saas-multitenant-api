# dotnet-saas-multitenant-api

A scalable multi-tenant SaaS backend built with ASP.NET Core (.NET 10). It supports tenant-based isolation and dynamic external service integrations per tenant. Designed with Clean Architecture and powered by Polly for resilience, it provides a flexible, extensible, and production-ready foundation for modern SaaS applications.

## Features

- **Multi-Tenancy**: Built-in tenant resolution and data isolation
- **Clean Architecture**: Clear separation between Domain, Application, Infrastructure, and Presentation layers
- **Domain-Driven Design**: Rich domain models with entities, value objects, and domain events
- **.NET 10**: Built on the latest .NET platform
- **Swagger/OpenAPI**: Interactive API documentation
- **Serilog**: Structured logging for better observability
- **Polly**: Resilience and transient fault handling
- **Unit & Integration Tests**: Comprehensive test coverage

## Project Structure

```
├── source/
│   ├── Domain/           # Core business logic and entities
│   ├── Application/      # Use cases and application services
│   ├── Infrastructure/   # Data access, external services
│   └── WebAPI/           # REST API endpoints
├── Tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── Application.IntegrationTests/
│   ├── Infrastructure.IntegrationTests/
│   └── API.IntegrationTests/
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Your preferred IDE (Visual Studio 2022+, VS Code, Rider)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/MofaggolHoshen/dotnet-saas-multitenant-api.git
cd dotnet-saas-multitenant-api
```

### Build the Solution

```bash
dotnet build
```

### Run the API

```bash
cd source/WebAPI
dotnet run
```

The API will be available at `http://localhost:5000`

### Run Tests

```bash
dotnet test
```

## API Documentation

Once the application is running, access the Swagger UI at:
- `http://localhost:5000/swagger`

## Architecture

This solution follows **Clean Architecture** principles:

| Layer | Description |
|-------|-------------|
| **Domain** | Core business logic, entities, value objects, domain events |
| **Application** | Use cases, commands, queries, DTOs, interfaces |
| **Infrastructure** | Database, external services, implementations |
| **WebAPI** | Controllers, middleware, API configuration |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is open source and available under the [MIT License](LICENSE).
