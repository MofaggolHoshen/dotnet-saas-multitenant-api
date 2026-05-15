# API Testing Guide

This document provides comprehensive testing strategies and examples for Phase 9 API endpoints.

---

## Overview

Testing the API layer involves multiple levels:

1. **Unit Tests**: Test controllers in isolation (mock dependencies)
2. **Integration Tests**: Test full HTTP → Database flow
3. **Manual Testing**: Interactive testing with tools (Postman, curl, Swagger)
4. **Contract Tests**: Ensure API responses match expected schema

---

## Unit Testing Controllers

### Setup

Controller unit tests mock the MediatR sender to isolate HTTP response mapping logic.

**Test Project Structure:**

```
Tests/
  UnitTests/
    Presentation/
      Controllers/
        AuthControllerTests.cs
        UsersControllerTests.cs
```

### Example: AuthController Tests

**File:** `Tests/UnitTests/Presentation/Controllers/AuthControllerTests.cs`

```csharp
using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Domain.Common;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Presentation.API.Controllers;
using Xunit;

namespace UnitTests.Presentation.Controllers;

public class AuthControllerTests
{
    private readonly Mock<ISender> _mockSender;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockSender = new Mock<ISender>();
        _controller = new AuthController();

        // Inject mock sender via reflection (protected property)
        var senderProperty = typeof(ApiController)
            .GetProperty("Sender", BindingFlags.NonPublic | BindingFlags.Instance);
        senderProperty?.SetValue(_controller, _mockSender.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        // Arrange
        var loginCommand = new LoginCommand(
            Email: "test@example.com",
            Password: "password",
            TenantId: Guid.NewGuid()
        );

        var loginResponse = new LoginResponse(
            AccessToken: "test-access-token",
            RefreshToken: "test-refresh-token",
            ExpiresIn: 3600,
            TokenType: "Bearer"
        );

        _mockSender
            .Setup(s => s.Send(loginCommand, default))
            .ReturnsAsync(Result<LoginResponse>.Success(loginResponse));

        // Act
        var result = await _controller.Login(loginCommand);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<LoginResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(loginResponse);
        apiResponse.Data.AccessToken.Should().Be("test-access-token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        var loginCommand = new LoginCommand(
            Email: "test@example.com",
            Password: "wrong-password",
            TenantId: Guid.NewGuid()
        );

        var error = Error.Failure("Unauthorized", "Invalid email or password");
        _mockSender
            .Setup(s => s.Send(loginCommand, default))
            .ReturnsAsync(Result<LoginResponse>.Failure(error));

        // Act
        var result = await _controller.Login(loginCommand);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var apiResponse = unauthorizedResult.Value.Should().BeOfType<ApiResponse<LoginResponse>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Register_WithValidData_Returns200WithUserInfo()
    {
        // Arrange
        var registerCommand = new RegisterCommand(
            Email: "newuser@example.com",
            Password: "SecurePass123!",
            FullName: "Test User",
            TenantId: Guid.NewGuid()
        );

        var registerResponse = new RegisterResponse(
            UserId: Guid.NewGuid(),
            Email: "newuser@example.com",
            FullName: "Test User",
            TenantId: registerCommand.TenantId
        );

        _mockSender
            .Setup(s => s.Send(registerCommand, default))
            .ReturnsAsync(Result<RegisterResponse>.Success(registerResponse));

        // Act
        var result = await _controller.Register(registerCommand);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<RegisterResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Email.Should().Be("newuser@example.com");
    }
}
```

### Example: UsersController Tests

**File:** `Tests/UnitTests/Presentation/Controllers/UsersControllerTests.cs`

```csharp
public class UsersControllerTests
{
    private readonly Mock<ISender> _mockSender;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockSender = new Mock<ISender>();
        _controller = new UsersController();

        // Inject mock sender
        var senderProperty = typeof(ApiController)
            .GetProperty("Sender", BindingFlags.NonPublic | BindingFlags.Instance);
        senderProperty?.SetValue(_controller, _mockSender.Object);
    }

    [Fact]
    public async Task GetUsers_Returns200WithPaginatedList()
    {
        // Arrange
        var query = new GetUsersQuery(PageNumber: 1, PageSize: 10);
        var users = new List<UserDto>
        {
            new UserDto(
                Id: Guid.NewGuid(),
                Email: "user1@example.com",
                FullName: "User One",
                IsActive: true,
                CreatedAt: DateTime.UtcNow
            ),
            new UserDto(
                Id: Guid.NewGuid(),
                Email: "user2@example.com",
                FullName: "User Two",
                IsActive: true,
                CreatedAt: DateTime.UtcNow
            )
        };

        var paginatedResponse = PaginatedResponse<UserDto>.Create(
            users, 1, 10, users.Count);

        _mockSender
            .Setup(s => s.Send(query, default))
            .ReturnsAsync(Result<PaginatedResponse<UserDto>>.Success(paginatedResponse));

        // Act
        var result = await _controller.GetUsers(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should()
            .BeOfType<ApiResponse<PaginatedResponse<UserDto>>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserById_WithValidId_Returns200WithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDetail = new UserDetailDto(
            Id: userId,
            Email: "user@example.com",
            FullName: "Test User",
            IsActive: true,
            TenantId: Guid.NewGuid(),
            TenantName: "Test Tenant",
            Roles: new List<UserRoleDto>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            LastLoginAt: null
        );

        _mockSender
            .Setup(s => s.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(Result<UserDetailDto>.Success(userDetail));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<UserDetailDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.NotFound("User.NotFound", "User not found");

        _mockSender
            .Setup(s => s.Send(It.Is<GetUserByIdQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(Result<UserDetailDto>.Failure(error));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<UserDetailDto>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Update_WithMismatchedIds_Returns400BadRequest()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var commandId = Guid.NewGuid(); // Different ID
        var command = new UpdateUserCommand(
            UserId: commandId,
            FullName: "Updated Name",
            Email: null,
            IsActive: null
        );

        // Act
        var result = await _controller.Update(routeId, command);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var apiResponse = badRequestResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("Route ID does not match");
    }
}
```

---

## Integration Testing

Integration tests use `WebApplicationFactory` to spin up an in-memory API with a test database.

### Setup

**File:** `Tests/IntegrationTests/CustomWebApplicationFactory.cs`

```csharp
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build service provider and seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(ApplicationDbContext db)
    {
        // Seed tenants, users, roles for testing
        var tenant = new Tenant(Guid.NewGuid(), "Test Tenant");
        db.Tenants.Add(tenant);

        var adminRole = new Role(Guid.NewGuid(), "TenantAdmin", "Admin role");
        var userRole = new Role(Guid.NewGuid(), "User", "User role");
        db.Roles.AddRange(adminRole, userRole);

        var admin = new User(
            Guid.NewGuid(),
            Email.Create("admin@tenant1.com").Value,
            FullName.Create("Admin User").Value,
            PasswordHash.Create("hashed-password").Value,
            tenant.Id
        );
        db.Users.Add(admin);

        db.SaveChanges();
    }
}
```

### Example: Authentication Endpoint Tests

**File:** `Tests/IntegrationTests/API/AuthEndpointsTests.cs`

```csharp
using System.Net;
using System.Net.Http.Json;
using Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace IntegrationTests.API;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var request = new
        {
            email = "admin@tenant1.com",
            password = "Admin123!",
            tenantId = _factory.TestTenantId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        var request = new
        {
            email = "admin@tenant1.com",
            password = "WrongPassword123!",
            tenantId = _factory.TestTenantId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    [Fact]
    public async Task Register_WithValidData_CreatesUserAndReturns200()
    {
        // Arrange
        var request = new
        {
            email = $"newuser{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            fullName = "New User",
            tenantId = _factory.TestTenantId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(request.email);
        result.Data.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var request = new
        {
            email = "admin@tenant1.com", // Already exists
            password = "SecurePass123!",
            fullName = "Duplicate User",
            tenantId = _factory.TestTenantId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }
}
```

### Example: User Endpoints Tests

**File:** `Tests/IntegrationTests/API/UserEndpointsTests.cs`

```csharp
public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private string _adminToken;

    public UserEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _adminToken = GetAdminTokenAsync().Result;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new
        {
            email = "admin@tenant1.com",
            password = "Admin123!",
            tenantId = _factory.TestTenantId
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        return result!.Data!.AccessToken;
    }

    [Fact]
    public async Task GetUsers_AsAuthenticatedUser_ReturnsPaginatedList()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        // Act
        var response = await _client.GetAsync("/api/v1/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<PaginatedResponse<UserDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_Returns401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_AsAdmin_CreatesUserSuccessfully()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var request = new
        {
            email = $"newuser{Guid.NewGuid()}@example.com",
            password = "SecurePass123!",
            fullName = "Integration Test User",
            initialRoles = new[] { "User" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(request.email);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var userId = _factory.TestUserId;
        var request = new
        {
            userId = userId,
            fullName = "Updated Name",
            email = "updated@example.com",
            isActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/users/{userId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("Updated Name");
    }
}
```

---

## Manual Testing with Postman

### Postman Collection Setup

**Collection Variables:**

```json
{
  "baseUrl": "http://localhost:5000",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accessToken": "",
  "refreshToken": ""
}
```

### 1. Login Request

```
POST {{baseUrl}}/api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@tenant1.com",
  "password": "Admin123!",
  "tenantId": "{{tenantId}}"
}
```

**Post-Response Script (Auto-save token):**

```javascript
if (pm.response.code === 200) {
  const response = pm.response.json();
  pm.collectionVariables.set("accessToken", response.data.accessToken);
  pm.collectionVariables.set("refreshToken", response.data.refreshToken);
}
```

### 2. Get Users Request

```
GET {{baseUrl}}/api/v1/users?pageNumber=1&pageSize=10
Authorization: Bearer {{accessToken}}
```

### 3. Create User Request

```
POST {{baseUrl}}/api/v1/users
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "fullName": "Postman Test User",
  "initialRoles": ["User"]
}
```

---

## Manual Testing with cURL

### 1. Login

```bash
# Login and save response to file
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@tenant1.com",
    "password": "Admin123!",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }' \
  -o login_response.json

# Extract token using jq
export TOKEN=$(cat login_response.json | jq -r '.data.accessToken')
```

### 2. Get Users

```bash
curl -X GET "http://localhost:5000/api/v1/users?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN" \
  | jq .
```

### 3. Create User

```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePass123!",
    "fullName": "cURL Test User",
    "initialRoles": ["User"]
  }' \
  | jq .
```

### 4. Get User By ID

```bash
USER_ID="3fa85f64-5717-4562-b3fc-2c963f66afa6"

curl -X GET "http://localhost:5000/api/v1/users/$USER_ID" \
  -H "Authorization: Bearer $TOKEN" \
  | jq .
```

---

## Swagger/OpenAPI Testing

### Access Swagger UI

```
http://localhost:5000/swagger
```

### Authorize in Swagger

1. Click **"Authorize"** button
2. Enter: `Bearer <your-access-token>`
3. Click **"Authorize"**
4. Close dialog

Now all subsequent requests include the token automatically.

### Test Endpoints

1. Expand `/api/v1/auth/login`
2. Click **"Try it out"**
3. Fill in request body
4. Click **"Execute"**
5. View response below

---

## Performance Testing

### Load Test with Apache Bench

```bash
# Login endpoint (100 requests, 10 concurrent)
ab -n 100 -c 10 -p login_payload.json -T application/json \
  http://localhost:5000/api/v1/auth/login

# Get users endpoint (with token)
ab -n 100 -c 10 \
  -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/v1/users
```

### Load Test with k6

**File:** `tests/load/login_test.js`

```javascript
import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 10,
  duration: "30s",
};

export default function () {
  const payload = JSON.stringify({
    email: "admin@tenant1.com",
    password: "Admin123!",
    tenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  });

  const params = {
    headers: { "Content-Type": "application/json" },
  };

  const response = http.post(
    "http://localhost:5000/api/v1/auth/login",
    payload,
    params,
  );

  check(response, {
    "status is 200": (r) => r.status === 200,
    "has access token": (r) => JSON.parse(r.body).data.accessToken !== null,
  });
}
```

**Run:**

```bash
k6 run tests/load/login_test.js
```

---

## Contract Testing

### Schema Validation

Ensure API responses match expected JSON schema.

**File:** `Tests/ContractTests/AuthResponseSchemaTests.cs`

```csharp
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;

namespace ContractTests;

public class AuthResponseSchemaTests
{
    private readonly JSchema _loginResponseSchema = JSchema.Parse(@"{
      'type': 'object',
      'properties': {
        'success': { 'type': 'boolean' },
        'data': {
          'type': 'object',
          'properties': {
            'accessToken': { 'type': 'string' },
            'refreshToken': { 'type': 'string' },
            'expiresIn': { 'type': 'integer' },
            'tokenType': { 'type': 'string' }
          },
          'required': ['accessToken', 'refreshToken', 'expiresIn', 'tokenType']
        },
        'message': { 'type': ['string', 'null'] },
        'timestamp': { 'type': 'string' }
      },
      'required': ['success', 'data', 'timestamp']
    }");

    [Fact]
    public async Task LoginResponse_ShouldMatchSchema()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { email = "admin@tenant1.com", password = "Admin123!", tenantId = _tenantId };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        var json = await response.Content.ReadAsStringAsync();
        var jObject = JObject.Parse(json);

        // Assert
        jObject.IsValid(_loginResponseSchema).Should().BeTrue();
    }
}
```

---

## Smoke Tests

Quick sanity checks after deployment.

**Bash Script:** `tests/smoke/api_smoke_test.sh`

```bash
#!/bin/bash

BASE_URL="${BASE_URL:-http://localhost:5000}"
EXIT_CODE=0

echo "Running API Smoke Tests..."

# Test 1: Health check (if implemented)
echo "Test: Health endpoint..."
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" $BASE_URL/health)
if [ "$RESPONSE" != "200" ]; then
  echo "❌ Health check failed (HTTP $RESPONSE)"
  EXIT_CODE=1
else
  echo "✅ Health check passed"
fi

# Test 2: Login
echo "Test: Login endpoint..."
LOGIN_RESPONSE=$(curl -s -X POST $BASE_URL/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@tenant1.com","password":"Admin123!","tenantId":"3fa85f64-5717-4562-b3fc-2c963f66afa6"}')

TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.accessToken')
if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
  echo "❌ Login failed"
  EXIT_CODE=1
else
  echo "✅ Login successful"
fi

# Test 3: Get users
echo "Test: Get users endpoint..."
USERS_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer $TOKEN" \
  $BASE_URL/api/v1/users)

if [ "$USERS_RESPONSE" != "200" ]; then
  echo "❌ Get users failed (HTTP $USERS_RESPONSE)"
  EXIT_CODE=1
else
  echo "✅ Get users successful"
fi

echo "Smoke tests complete!"
exit $EXIT_CODE
```

**Run:**

```bash
chmod +x tests/smoke/api_smoke_test.sh
./tests/smoke/api_smoke_test.sh
```

---

## Summary

### Testing Levels

| Level       | Tool                  | Purpose                          |
| ----------- | --------------------- | -------------------------------- |
| Unit        | xUnit + Moq           | Test controllers in isolation    |
| Integration | WebApplicationFactory | Test full HTTP flow with real DB |
| Manual      | Postman/cURL          | Interactive exploratory testing  |
| Contract    | JSON Schema           | Verify response structure        |
| Load        | k6/Apache Bench       | Measure performance under load   |
| Smoke       | Bash scripts          | Quick deployment validation      |

### Best Practices

1. **Automate regression tests**: Integration tests catch breaking changes
2. **Mock external dependencies**: Unit tests stay fast and reliable
3. **Test auth flows thoroughly**: Security is critical
4. **Validate response schemas**: Prevent contract breakage
5. **Monitor performance**: Detect degradation early

---

## Next Steps

- Add health check endpoint for monitoring
- Implement rate limiting on auth endpoints
- Set up CI/CD pipeline with automated tests
- Configure Swagger UI for production (with authentication)
- Add request/response logging middleware

Comprehensive testing ensures API reliability and maintainability.
