using Microsoft.EntityFrameworkCore;
using WhatsAppSendingService.Application;
using WhatsAppSendingService.Infrastructure;
using WhatsAppSendingService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-migrate in dev for a friction-free first run.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed so the BDD/integration tests can reference the entry-point assembly.
public partial class Program;
