# WebAPI Layer (Presentation)

The WebAPI layer is the entry point for the application, handling HTTP requests and responses. It provides RESTful API endpoints for client applications.

## Purpose

This layer exposes the application functionality through HTTP endpoints. It handles request/response formatting, authentication, authorization, and API documentation.

## Structure

```
WebAPI/
├── Controllers/          # API controllers
├── Middleware/           # Custom middleware (error handling, tenant resolution)
├── Filters/              # Action filters (validation, authorization)
├── Extensions/           # Service configuration extensions
└── Program.cs            # Application entry point and configuration
```

## Key Components

### Controllers
- Thin controllers that delegate to MediatR
- Handle HTTP-specific concerns only
- Return appropriate HTTP status codes

### Middleware
- **ExceptionHandlingMiddleware**: Global exception handling
- **TenantResolutionMiddleware**: Multi-tenancy tenant resolution
- **RequestLoggingMiddleware**: Request/response logging

### Configuration
- JWT authentication setup
- Swagger/OpenAPI documentation
- Serilog logging configuration
- Dependency injection setup

## Dependencies

- **Infrastructure** - References infrastructure for DI setup
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **Serilog.AspNetCore** - Structured logging
- **Serilog.Sinks.Console** - Console logging sink

## API Design Guidelines

- Use RESTful conventions
- Return appropriate HTTP status codes
- Use ProblemDetails for error responses
- Version APIs when making breaking changes
- Document all endpoints with Swagger

## Running the API

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`
