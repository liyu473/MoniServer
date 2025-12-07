using Grpc.Net.Client;
using Jab;
using LyuMonion.JwtAuth.Client;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MoniClient.Service;

[ServiceProvider]
[Import<IUtilitiesModule>]
[Singleton<MainWindowViewModel>]
[Singleton<TokenStore>]
[Singleton(typeof(IConfiguration), Factory = nameof(BuildConfig))]
[Singleton(typeof(GrpcChannel), Factory = nameof(BuildChannel))]
public partial class JabService
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static GrpcChannel BuildChannel(IConfiguration config, TokenStore tokenStore) =>
        GrpcChannelBuilder.Create(config["MagicOnion:ServerUrl"] ?? "http://localhost:5000")
            .WithJwtAuth(tokenStore)
            .Build();
}
