using Application;
using Infrastructure;
using Infrastructure.Multitenancy;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    _ = await dbContext.Database.CanConnectAsync();
}

app.UseHttpsRedirection();

// Tenant resolution middleware - must be after routing and before authorization
app.UseTenantResolution();

app.UseAuthorization();

app.MapControllers();

app.Run();
