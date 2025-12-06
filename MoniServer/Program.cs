using LogExtension.Builder;
using LogExtension.Extensions;
using LyuMonionCore.Server;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory }
);

// Self Log Nuget Package
builder.Services.AddZLogger(builder =>
    builder
        .WithConsoleDetails()
        .WithConsoleFilter("Microsoft", LogLevel.Information)
        .FilterMicrosoft()
);

builder.Services.AddMagicOnion();

builder.Services.AddMonionNotification();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// 官方支持取代Swagger
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

builder
    .Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapMagicOnionService();

app.Run();
