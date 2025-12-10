using LogExtension.Builder;
using LogExtension.Extensions;
using LyuMonion.JwtAuth.Server;
using LyuMonionCore.Server;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory }
);

var logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");

// Self Log Nuget Package
builder.Services.AddZLogger(builder =>
    builder
        .WithConsoleDetails()
        .WithConsoleFilter("Grpc", LogLevel.Warning)
        .WithConsoleFilter("Microsoft", LogLevel.Warning)
        .WithConsoleFilter("MagicOnion", LogLevel.Warning)
        .FilterMicrosoft()
        .WithFilter("Grpc", LogLevel.Warning)
        .WithFilter("MagicOnion", LogLevel.Warning)
        .AddFileOutput(logsPath)
);

builder
    .Services.AddMagicOnion()
    .AddJwtAuth(options =>
    {
        options.SecretKey = "YourSuperSecretKeyAtLeast32Characters!";
        options.Issuer = "MoniServer";
        options.Audience = "MoniClient";
        options.ExpiresInMinutes = 5;//无偏差
        options.ExcludeServices("IAuthService"); // 登录接口不需要验证
        options.OnTokenValidated = async (context, serviceProvider) =>
        {
            // 额外的认证逻辑（使用场景：踢人下线）
            // context.Fail("测试失败");
        }; 
    });

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
