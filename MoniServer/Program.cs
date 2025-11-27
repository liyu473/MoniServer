using LogExtension;
using MonionCore.Notification;
using MoniServer.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory
});

// Self Log Nuget Package
builder.Services.AddZLogger(config =>
{
    var logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");
    config.InfoLogPath = logsPath + Path.DirectorySeparatorChar;
    config.TraceLogPath = Path.Combine(logsPath, "trace") + Path.DirectorySeparatorChar;

    // 对trace文件夹不生效
    config.CategoryFilters["Microsoft"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.AspNetCore"] = LogLevel.Warning;
    config.CategoryFilters["Microsoft.Hosting.Lifetime"] = LogLevel.Information;
    config.CategoryFilters["MagicOnion.Server"] = LogLevel.Warning;
    
    // 控制台输出
    config.AdditionalConfiguration = logging =>
    {
        logging.AddZLoggerConsoleWithTimestamp();
    };
});

builder.Services.AddMagicOnion();


builder.Services.AddSingleton<INotificationPushService, NotificationPushService>();

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
